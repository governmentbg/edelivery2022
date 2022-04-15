using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Options;
using static ED.Domain.IEsbMessagesSendQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    internal record EsbSendMessageCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        IEncryptorFactory EncryptorFactory,
        EncryptorFactoryV1 EncryptorFactoryV1,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        TimestampServiceClient TimestampServiceClient,
        OrnServiceClient OrnServiceClient,
        IEsbMessagesSendQueryRepository EsbMessagesSendQueryRepository,
        IProfilesService ProfilesService,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<EsbSendMessageCommand, int>
    {
        private const string NotificationEvent = "OnSend";

        public async Task<int> Handle(
            EsbSendMessageCommand command,
            CancellationToken ct)
        {
            // TODO: refactor
            using IEncryptor encryptor = this.EncryptorFactory.CreateEncryptor();

            int[] distinctRecipientProfileIds = command.RecipientProfileIds
                .Distinct()
                .ToArray();

            int[] allProfileIds = distinctRecipientProfileIds
                .Concat(new int[] { command.SenderProfileId })
                .Distinct() // in chase the sender is also among the recipients
                .ToArray();

            GetProfileNamesVO[] profileNames =
                await this.EsbMessagesSendQueryRepository.GetProfileNamesAsync(
                    allProfileIds,
                    ct);

            string[] recipientProfileNames =
                (from pn in profileNames
                 join rpId in command.RecipientProfileIds
                     on pn.ProfileId equals rpId
                 select pn.ProfileName)
                .ToArray();

            string recipientsAsText = string.Join(", ", recipientProfileNames);

            ProfileKeyVO[] profileKeys = new ProfileKeyVO[allProfileIds.Length];
            for (int i = 0; i < allProfileIds.Length; i++)
            {
                profileKeys[i] =
                    await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                        allProfileIds[i],
                        ct);
            }

            Message.MessageAccessKeyDO[] messageAccessKeys =
                await this.CreateMessageAccessKeysAsync(
                    profileKeys,
                    encryptor.Key,
                    ct);

            DecryptedTemporaryBlobVO[] decryptedTemporaryBlobs =
                (await this.GetTemporaryBlobsAsync(
                    command.SenderProfileId,
                    command.BlobIds,
                    ct))
                .ToArray();

            Message.MessageBlobDO[] messageBlobs =
                await this.CreateMessageBlobsAsync(
                    profileKeys,
                    command.BlobIds,
                    decryptedTemporaryBlobs,
                    ct);

            string senderIdentifier =
                await this.EsbMessagesSendQueryRepository.GetProfileIdentifierAsync(
                    command.SenderProfileId,
                    ct);

            string orn =
                await this.OrnServiceClient.SubmitAsync(senderIdentifier, ct);

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            Message message = new(
                command.SenderLoginId,
                command.SenderProfileId,
                distinctRecipientProfileIds,
                recipientsAsText,
                command.TemplateId,
                command.Subject,
                orn,
                command.ReferencedOrn,
                command.AdditionalIdentifier,
                this.HandleJson(encryptor, command.Body),
                command.MetaFields,
                command.CreatedBy,
                command.SenderViaLoginId,
                encryptor.IV,
                messageAccessKeys,
                messageBlobs);

            await this.MessageAggregateRepository.AddAsync(message, ct);

            await this.UnitOfWork.SaveAsync(ct);

            byte[]? forwardedMessageSummarySha256 = null;
            if (command.ForwardedMessageId.HasValue)
            {
                Message? forwardedMessage = null;

                Message forwardedMessage1 =
                    await this.MessageAggregateRepository.FindAsync(
                        command.ForwardedMessageId.Value,
                        ct);

                if (forwardedMessage1.Forwarded != null)
                {
                    forwardedMessage1.ForwardStatusId = ForwardStatus.IsInForwardChainAndForwarded;

                    forwardedMessage =
                        await this.MessageAggregateRepository.FindAsync(
                            forwardedMessage1.Forwarded.ForwardedMessageId,
                            ct);
                }
                else
                {
                    forwardedMessage = forwardedMessage1;
                }

                forwardedMessage.ForwardStatusId = ForwardStatus.IsOriginalForwarded;

                if (forwardedMessage.MessageSummaryVersion == MessageSummaryVersion.V1)
                {
                    using IEncryptor encryptorV1 = this.EncryptorFactoryV1.CreateEncryptor();
                    forwardedMessageSummarySha256 = SHA256.HashData(
                        encryptorV1.Decrypt(
                            forwardedMessage.MessageSummary
                            ?? throw new Exception("MessageSummary should not be null")));
                }
                else if (forwardedMessage.MessageSummaryVersion == MessageSummaryVersion.V2)
                {
                    using MemoryStream memoryStream = new(
                        forwardedMessage.MessageSummary
                        ?? throw new Exception("MessageSummary should not be null"));
                    forwardedMessageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(memoryStream);
                }
                else
                {
                    throw new Exception($"Unknown MessageSummaryVersion {forwardedMessage.MessageSummaryVersion}");
                }

                message.AddForwardedMessage(
                    forwardedMessage.MessageId,
                    forwardedMessage!.SubjectExtended!,
                    forwardedMessage.SenderProfile.Id,
                    forwardedMessage.SenderProfile.ElectronicSubjectName,
                    forwardedMessage.SenderProfile.ElectronicSubjectId,
                    forwardedMessage!.SenderLogin!.ElectronicSubjectName);

                // when forwarding a message there can be only one recipient
                int recipientProfileId = distinctRecipientProfileIds.First();

                bool recipientHasAlreadyMessageAccessKey = forwardedMessage
                    .MessageAccessKeys
                    .Any(mak => mak.ProfileId == recipientProfileId);

                if (!recipientHasAlreadyMessageAccessKey)
                {
                    ProfileKeyVO recipientProfileKey =
                        await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                            recipientProfileId,
                            ct);

                    MessageAccessKey senderMessageAccessKey = forwardedMessage
                        .MessageAccessKeys
                        .First(e => e.ProfileId == command.SenderProfileId);

                    ProfileKeyVO senderProfileKey =
                        await this.ProfilesService.GetProfileKeyAsync(
                            senderMessageAccessKey.ProfileKeyId,
                            ct);

                    byte[] forwardedDecryptedEncryptionKey = await this.DecryptMessageAccessKey(
                        senderProfileKey,
                        senderMessageAccessKey.EncryptedKey,
                        ct);

                    Message.MessageAccessKeyDO[] forwardedMessageAccessKeys =
                        await this.CreateMessageAccessKeysAsync(
                            new[] { recipientProfileKey },
                            forwardedDecryptedEncryptionKey,
                            ct);

                    int[] blobIds = forwardedMessage.MessageBlobs
                        .Select(e => e.BlobId)
                        .ToArray();

                    DecryptedMessageBlobVO[] forwardedDecryptedMessageBlobs =
                        (await this.GetMessageBlobsAsync(
                            command.SenderProfileId,
                            forwardedMessage.MessageId,
                            blobIds,
                            ct))
                        .ToArray();

                    Message.MessageBlobAccesKeyDO[] forwardedMessageBlobAccessKeys =
                        await this.CreateMessageBlobAccessKeysAsync(
                            new[] { recipientProfileKey },
                            forwardedDecryptedMessageBlobs,
                            ct);

                    forwardedMessage.AddRecipients(
                        forwardedMessageAccessKeys,
                        forwardedMessageBlobAccessKeys);
                }

                await this.UnitOfWork.SaveAsync(ct);
            }

            (string messageSummaryXml, byte[] messageSummary, byte[] sendTimestamp) =
                await this.CreateMessageSummary(
                    message.MessageId,
                    message.Orn,
                    message.ReferencedOrn,
                    message.AdditionalIdentifier,
                    command.SenderProfileId,
                    profileKeys,
                    profileNames,
                    message.DateSent,
                    message.Subject,
                    command.Body,
                    message.MetaFields,
                    message.TemplateId,
                    decryptedTemporaryBlobs,
                    command.ForwardedMessageId,
                    forwardedMessageSummarySha256,
                    ct);

            message.UpdateExtendedSubject(
                messageSummaryXml,
                messageSummary,
                sendTimestamp);

            await this.UnitOfWork.SaveAsync(ct);

            NotificationMessages notificationMessages =
                await this.GetNotificationsAsync(
                    message.MessageId,
                    profileNames.Single(pn => pn.ProfileId == command.SenderProfileId).ProfileName,
                    ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.EmailQueueMessages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.SmsQueueMessages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.PushNotificationQueueMessages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.ViberQueueMessages,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return message.MessageId;
        }

        private byte[] HandleJson(IEncryptor encryptor, string json)
        {
            string canonnicalizedJson = JsonCanonicalizationHelper.Canonicalize(json);
            return encryptor.Encrypt(Encoding.UTF8.GetBytes(canonnicalizedJson));
        }

        private record DecryptedTemporaryBlobVO(
            int BlobId,
            string FileName,
            string Hash,
            string HashAlgorith,
            byte[] DecryptedKey);

        private async Task<List<DecryptedTemporaryBlobVO>> GetTemporaryBlobsAsync(
            int senderProfileId,
            int[] blobIds,
            CancellationToken ct)
        {
            GetTemporaryOrStorageBlobsVO[] temporaryBlobs =
                await this.EsbMessagesSendQueryRepository.GetTemporaryOrStorageBlobsAsync(
                    senderProfileId,
                    blobIds,
                    ct);

            List<DecryptedTemporaryBlobVO> decryptedTemporaryBlobs = new();
            List<Task<DecryptedTemporaryBlobVO>> tasks = new();

            foreach (var item in temporaryBlobs)
            {
                tasks.Add(this.DecryptBlobKeyAndGetTimestampAsync(item, ct));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                decryptedTemporaryBlobs.Add(await task);
            }

            return decryptedTemporaryBlobs;
        }

        private async Task<Message.MessageBlobDO[]> CreateMessageBlobsAsync(
            ProfileKeyVO[] profileKeys,
            int[] blobIds,
            DecryptedTemporaryBlobVO[] temporaryBlobs,
            CancellationToken ct)
        {
            List<Message.MessageBlobDO> messageBlobs = new();
            List<Task<List<Message.MessageBlobDO>>> tasks = new();

            foreach (var item in profileKeys)
            {
                tasks.Add(
                    this.EncryptMessageBlobKeyAsync(
                        item,
                        blobIds,
                        temporaryBlobs,
                        ct));
            }

            await Task.WhenAll(tasks);

            foreach (var item in tasks)
            {
                messageBlobs.AddRange(await item);
            }

            return messageBlobs.ToArray();
        }

        private async Task<List<Message.MessageBlobDO>> EncryptMessageBlobKeyAsync(
            ProfileKeyVO profileKey,
            int[] blobIds,
            DecryptedTemporaryBlobVO[] temporaryBlobs,
            CancellationToken ct)
        {
            List<Message.MessageBlobDO> messageBlobPerProfile = new();

            for (int i = 0; i < blobIds.Length; i++)
            {
                int blobId = blobIds[i];
                DecryptedTemporaryBlobVO blob =
                    temporaryBlobs.First(e => e.BlobId == blobId);

                Keystore.EncryptWithRsaKeyResponse encryptedKeyResp =
                    await this.KeystoreClient.EncryptWithRsaKeyAsync(
                        request: new Keystore.EncryptWithRsaKeyRequest
                        {
                            Key = new Keystore.RsaKey
                            {
                                Provider = profileKey.Provider,
                                KeyName = profileKey.KeyName,
                                OaepPadding = profileKey.OaepPadding,
                            },
                            Plaintext = ByteString.CopyFrom(blob.DecryptedKey),
                        },
                        cancellationToken: ct);

                messageBlobPerProfile.Add(
                    new Message.MessageBlobDO(
                        i,
                        profileKey.ProfileId,
                        profileKey.ProfileKeyId,
                        blob.BlobId,
                        encryptedKeyResp.EncryptedData.ToByteArray()));
            }

            return messageBlobPerProfile;
        }

        private async Task<DecryptedTemporaryBlobVO> DecryptBlobKeyAndGetTimestampAsync(
            GetTemporaryOrStorageBlobsVO temporaryBlob,
            CancellationToken ct)
        {
            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
                await this.KeystoreClient.DecryptWithRsaKeyAsync(
                    request: new Keystore.DecryptWithRsaKeyRequest
                    {
                        Key = new Keystore.RsaKey
                        {
                            Provider = temporaryBlob.Provider,
                            KeyName = temporaryBlob.KeyName,
                            OaepPadding = temporaryBlob.OaepPadding,
                        },
                        EncryptedData = ByteString.CopyFrom(temporaryBlob.EncryptedKey)
                    },
                    cancellationToken: ct);

            return new DecryptedTemporaryBlobVO(
                temporaryBlob.BlobId,
                temporaryBlob.FileName,
                temporaryBlob.Hash,
                temporaryBlob.HashAlgorith,
                decryptedKeyResp.Plaintext.ToByteArray());
        }

        private async Task<Message.MessageAccessKeyDO[]> CreateMessageAccessKeysAsync(
            ProfileKeyVO[] profileKeys,
            byte[] encryptionKey,
            CancellationToken ct)
        {
            List<Message.MessageAccessKeyDO> messageAccessKeys = new();
            List<Task<Message.MessageAccessKeyDO>> tasks = new();

            foreach (var profileKey in profileKeys)
            {
                tasks.Add(
                    this.EncryptMessageKeyAsync(
                        profileKey,
                        encryptionKey,
                        ct));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                messageAccessKeys.Add(await task);
            }

            return messageAccessKeys.ToArray();
        }

        private async Task<Message.MessageAccessKeyDO> EncryptMessageKeyAsync(
            ProfileKeyVO profileKey,
            byte[] encryptionKey,
            CancellationToken ct)
        {
            Keystore.EncryptWithRsaKeyResponse encryptedKeyResp =
                await this.KeystoreClient.EncryptWithRsaKeyAsync(
                    request: new Keystore.EncryptWithRsaKeyRequest
                    {
                        Key = new Keystore.RsaKey
                        {
                            Provider = profileKey.Provider,
                            KeyName = profileKey.KeyName,
                            OaepPadding = profileKey.OaepPadding,
                        },
                        Plaintext = ByteString.CopyFrom(encryptionKey),
                    },
                    cancellationToken: ct);

            return new Message.MessageAccessKeyDO(
                profileKey.ProfileId,
                profileKey.ProfileKeyId,
                encryptedKeyResp.EncryptedData.ToByteArray());
        }

        private async Task<(string, byte[], byte[])> CreateMessageSummary(
            int messageId,
            string? orn,
            string? referencedOrn,
            string? additionalIdentifier,
            int senderProfileId,
            ProfileKeyVO[] allProfiles,
            GetProfileNamesVO[] allProfileNames,
            DateTime? dateSent,
            string subject,
            string? body,
            string? metaFields,
            int? templateId,
            DecryptedTemporaryBlobVO[] blobs,
            int? forwardedMessageId,
            byte[]? forwardedMessageSummarySha256,
            CancellationToken ct)
        {
            ProfileKeyVO senderProfileKey = allProfiles
                .First(e => e.ProfileId == senderProfileId);

            string senderProfileName = allProfileNames
                .Single(e => e.ProfileId == senderProfileId).ProfileName;

            ProfileKeyVO[] recipientProfileKeys = allProfiles
                .Where(e => e.ProfileId != senderProfileId)
                .ToArray();

            GetProfileNamesVO[] recipientProfileNames = allProfileNames
                .Where(e => e.ProfileId != senderProfileId)
                .ToArray();

            Message.MessageSummaryDO.MessageSummaryDOHashProperty hashBody = new(
                EncryptionHelper.GetSha256HashAsHexString(body!),
                EncryptionHelper.Sha256Algorithm);

            Message.MessageSummaryDO.MessageSummaryDOHashProperty hashMetaFields = new(
                EncryptionHelper.GetSha256HashAsHexString(metaFields!),
                EncryptionHelper.Sha256Algorithm);

            Message.MessageSummaryDO messageSummaryDO = new(
                messageId,
                orn,
                referencedOrn,
                additionalIdentifier,
                new Message.MessageSummaryDO.MessageSummaryVOProfile(
                    senderProfileId,
                    senderProfileName),
                recipientProfileNames
                    .Select(e => new Message.MessageSummaryDO.MessageSummaryVOProfile(
                        e.ProfileId,
                        e.ProfileName))
                    .ToArray(),
                dateSent,
                subject,
                hashBody,
                hashMetaFields,
                templateId!.Value,
                blobs
                    .Select(e => new Message.MessageSummaryDO.MessageSummaryDOAttachment(
                        e.Hash,
                        e.HashAlgorith,
                        e.FileName,
                        string.Empty))
                    .ToArray(),
                forwardedMessageId != null
                    ? new Message.MessageSummaryDO.MessageSummaryDOForwardedMessage(
                        forwardedMessageId.Value,
                        EncryptionHelper.GetHexString(forwardedMessageSummarySha256!),
                        EncryptionHelper.Sha256Algorithm)
                    : null);

            XmlSerializer serializer = new(typeof(Message.MessageSummaryDO));
            using MemoryStream memoryStream = new();
            using XmlWriter xmlWriter = XmlWriter.Create(
                memoryStream,
                new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Encoding = Encoding.UTF8
                });

            serializer.Serialize(xmlWriter, messageSummaryDO);

            string xml = Encoding.UTF8.GetString(memoryStream.ToArray());

            memoryStream.Position = 0;

            byte[] timestamp = await this.TimestampServiceClient.SubmitAsync(
                messageId,
                EncryptionHelper.Sha256Algorithm,
                XmlCanonicalizationHelper.GetSha256Hash(memoryStream),
                ct);

            return (xml, Encoding.UTF8.GetBytes(xml), timestamp);
        }

        private record DecryptedMessageBlobVO(
            int MessageBlobId,
            byte[] DecryptedKey);

        private async Task<byte[]> DecryptMessageAccessKey(
            ProfileKeyVO profileKey,
            byte[] encryptionKey,
            CancellationToken ct)
        {
            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
                await this.KeystoreClient.DecryptWithRsaKeyAsync(
                    request: new Keystore.DecryptWithRsaKeyRequest
                    {
                        Key = new Keystore.RsaKey
                        {
                            Provider = profileKey.Provider,
                            KeyName = profileKey.KeyName,
                            OaepPadding = profileKey.OaepPadding,
                        },
                        EncryptedData = ByteString.CopyFrom(encryptionKey)
                    },
                    cancellationToken: ct);

            return decryptedKeyResp.Plaintext.ToByteArray();
        }

        private async Task<List<DecryptedMessageBlobVO>> GetMessageBlobsAsync(
            int senderProfileId,
            int messageId,
            int[] blobIds,
            CancellationToken ct)
        {
            GetMessageBlobsVO[] messageBlobs =
                await this.EsbMessagesSendQueryRepository.GetMessageBlobsAsync(
                    senderProfileId,
                    messageId,
                    blobIds,
                    ct);

            List<DecryptedMessageBlobVO> decryptedMessageBlobs = new();
            List<Task<DecryptedMessageBlobVO>> tasks = new();

            foreach (var item in messageBlobs)
            {
                tasks.Add(this.DecryptBlobKeyAndGetTimestampAsync(item, ct));
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                decryptedMessageBlobs.Add(await task);
            }

            return decryptedMessageBlobs;
        }

        private async Task<DecryptedMessageBlobVO> DecryptBlobKeyAndGetTimestampAsync(
            GetMessageBlobsVO messageBlob,
             CancellationToken ct)
        {
            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
                await this.KeystoreClient.DecryptWithRsaKeyAsync(
                    request: new Keystore.DecryptWithRsaKeyRequest
                    {
                        Key = new Keystore.RsaKey
                        {
                            Provider = messageBlob.Provider,
                            KeyName = messageBlob.KeyName,
                            OaepPadding = messageBlob.OaepPadding,
                        },
                        EncryptedData = ByteString.CopyFrom(messageBlob.EncryptedKey)
                    },
                    cancellationToken: ct);

            return new DecryptedMessageBlobVO(
                messageBlob.MessageBlobId,
                decryptedKeyResp.Plaintext.ToByteArray());
        }

        private async Task<Message.MessageBlobAccesKeyDO[]> CreateMessageBlobAccessKeysAsync(
            ProfileKeyVO[] profileKeys,
            DecryptedMessageBlobVO[] messageBlobs,
            CancellationToken ct)
        {
            List<Message.MessageBlobAccesKeyDO> messageBlobsAccessKeys = new();
            List<Task<List<Message.MessageBlobAccesKeyDO>>> tasks = new();

            foreach (var profileKey in profileKeys)
            {
                tasks.Add(
                    this.EncryptMessageBlobKeyAsync(
                        profileKey,
                        messageBlobs,
                        ct));
            }

            await Task.WhenAll(tasks);

            foreach (var item in tasks)
            {
                messageBlobsAccessKeys.AddRange(await item);
            }

            return messageBlobsAccessKeys.ToArray();
        }

        private async Task<List<Message.MessageBlobAccesKeyDO>> EncryptMessageBlobKeyAsync(
            ProfileKeyVO profileKey,
            DecryptedMessageBlobVO[] messageBlobs,
            CancellationToken ct)
        {
            List<Message.MessageBlobAccesKeyDO> messageBlobAccessKeysPerProfile = new();

            foreach (DecryptedMessageBlobVO messageBlob in messageBlobs)
            {
                Keystore.EncryptWithRsaKeyResponse encryptedKeyResp =
                    await this.KeystoreClient.EncryptWithRsaKeyAsync(
                        request: new Keystore.EncryptWithRsaKeyRequest
                        {
                            Key = new Keystore.RsaKey
                            {
                                Provider = profileKey.Provider,
                                KeyName = profileKey.KeyName,
                                OaepPadding = profileKey.OaepPadding,
                            },
                            Plaintext = ByteString.CopyFrom(messageBlob.DecryptedKey),
                        },
                        cancellationToken: ct);

                messageBlobAccessKeysPerProfile.Add(
                    new Message.MessageBlobAccesKeyDO(
                        messageBlob.MessageBlobId,
                        profileKey.ProfileId,
                        profileKey.ProfileKeyId,
                        encryptedKeyResp.EncryptedData.ToByteArray()));
            }

            return messageBlobAccessKeysPerProfile;
        }

        private record NotificationMessages(
            EmailQueueMessage[] EmailQueueMessages,
            SmsQueueMessage[] SmsQueueMessages,
            PushNotificationQueueMessage[] PushNotificationQueueMessages,
            ViberQueueMessage[] ViberQueueMessages);

        private async Task<NotificationMessages> GetNotificationsAsync(
            int messageId,
            string senderProfileName,
            CancellationToken ct)
        {
            GetNotificationRecipientsVO[] notificationRecipients =
                await this.EsbMessagesSendQueryRepository
                    .GetNotificationRecipientsAsync(
                        messageId,
                        ct);

            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedEmailBody)}");

            string smsBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedSMSBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedSMSBody)}");

            string viberBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedViberBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedViberBody)}");

            string webPortalUrl =
                this.DomainOptionsAccessor.Value.WebPortalUrl
                ?? throw new Exception(
                    $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

            EmailQueueMessage[] emailRecipients = notificationRecipients
                .Where(e => e.IsEmailNotificationEnabled)
                .Select(e => new EmailQueueMessage(
                    e.Email,
                    emailSubect,
                    string.Format(
                        emailBody,
                        e.LoginName,
                        e.ProfileName,
                        senderProfileName,
                        webPortalUrl),
                    false,
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId
                    }))
                .ToArray();

            SmsQueueMessage[] smsRecipients = notificationRecipients
                .Where(e => e.IsSmsNotificationEnabled)
                .Select(e => new SmsQueueMessage(
                    e.Phone,
                    string.Format(
                        smsBody,
                        webPortalUrl,
                        ResourceHelper
                            .CyrillicToLatin(e.ProfileName)
                            .ToUpperInvariant(),
                        ResourceHelper
                            .CyrillicToLatin(senderProfileName)
                            .ToUpperInvariant()),
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId
                    }))
                .ToArray();

            PushNotificationQueueMessage[] pushNotificationRecipients = notificationRecipients
                .Where(e => !string.IsNullOrEmpty(e.PushNotificationUrl))
                .Select(e => new PushNotificationQueueMessage(
                    e.PushNotificationUrl!,
                    new
                    {
                        EventType = "Received",
                        MessageId = messageId
                    },
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId
                    }))
                .ToArray();

            ViberQueueMessage[] viberRecipients = notificationRecipients
                .Where(e => e.IsViberNotificationEnabled)
                .Select(e => new ViberQueueMessage(
                    e.Phone,
                    string.Format(
                        viberBody,
                        webPortalUrl,
                        e.ProfileName.ToUpperInvariant(),
                        senderProfileName.ToUpperInvariant()),
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId
                    }))
                .ToArray();

            return new NotificationMessages(
                emailRecipients,
                smsRecipients,
                pushNotificationRecipients,
                viberRecipients);
        }
    }
}
