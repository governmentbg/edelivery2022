using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Options;
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    internal record SendMessage1OnBehalfOfToLegalEntityCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IEncryptorFactory EncryptorFactory,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        TimestampServiceClient TimestampServiceClient,
        IIntegrationServiceMessagesSendQueryRepository IntegrationServiceMessagesSendQueryRepository,
        IProfilesService ProfilesService,
        IRnuService RnuService,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<SendMessage1OnBehalfOfToLegalEntityCommand, SendMessage1OnBehalfOfToLegalEntityCommandResult>
    {
        private const string NotificationEvent = "IntegrationService:{0}:OnSend";

        public async Task<SendMessage1OnBehalfOfToLegalEntityCommandResult> Handle(
            SendMessage1OnBehalfOfToLegalEntityCommand command,
            CancellationToken ct)
        {
            bool checkLoginSendOnBehalfOf =
                await this.IntegrationServiceMessagesSendQueryRepository
                    .CheckLoginSendOnBehalfOfAsync(
                        command.SentViaLoginId,
                        ct);

            if (!checkLoginSendOnBehalfOf)
            {
                return new(
                    false,
                    "The login is not allowed to send on behalf of other users!",
                    null);
            }

            GetRecipientOrSenderProfileVO? recipient =
                await this.IntegrationServiceMessagesSendQueryRepository
                    .GetRecipientOrSenderProfileAsync(
                        command.RecipientIdentifier,
                        new int[]
                        {
                            TargetGroup.LegalEntityTargetGroupId ,
                            TargetGroup.PublicAdministrationTargetGroupId,
                            TargetGroup.SocialOrganizationTargetGroupId
                        },
                        ct);

            if (recipient == null)
            {
                return new(
                    false,
                    "The requested recipient profile does not exist or is deactivated!", // copied from old implementation
                    null);
            }

            GetRecipientOrSenderProfileVO? sender =
                await this.IntegrationServiceMessagesSendQueryRepository
                    .GetRecipientOrSenderProfileAsync(
                        command.SenderIdentifier,
                        new int[]
                        {
                            TargetGroup.LegalEntityTargetGroupId ,
                            TargetGroup.PublicAdministrationTargetGroupId,
                            TargetGroup.SocialOrganizationTargetGroupId
                        },
                        ct);

            if (sender == null)
            {
                return new(
                    false,
                    "The requested sender profile does not exist or is deactivated!", // copied from old implementation
                    null);
            }

            using IEncryptor encryptor = this.EncryptorFactory.CreateEncryptor();

            GetProfileNamesVO[] profileNames = new GetProfileNamesVO[]
            {
                new GetProfileNamesVO(recipient.ProfileId, recipient.ProfileName),
                new GetProfileNamesVO(sender.ProfileId, sender.ProfileName),
            };

            int[] allProfileIds = new int[]
            {
                recipient.ProfileId,
                sender.ProfileId
            };

            ProfileKeyVO[] profileKeys = new ProfileKeyVO[allProfileIds.Length];
            for (int i = 0; i < allProfileIds.Length; i++)
            {
                profileKeys[i] =
                    await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                        allProfileIds[i],
                        ct);
            }

            ProfileKeyVO senderProfileKey = profileKeys
                .First(e => e.ProfileId == sender.ProfileId);

            Message.MessageAccessKeyDO[] messageAccessKeys =
                await this.CreateMessageAccessKeysAsync(
                    profileKeys,
                    encryptor.Key,
                    ct);

            int[] blobIds = command.Blobs.Select(x => x.BlobId).ToArray();

            DecryptedTemporaryBlobVO[] decryptedTemporaryBlobs =
                (await this.GetTemporaryBlobsAsync(
                    Profile.SystemProfileId,
                    blobIds,
                    ct))
                .ToArray();

            Message.MessageBlobDO[] messageBlobs =
                await this.CreateMessageBlobsAsync(
                    profileKeys,
                    blobIds,
                    decryptedTemporaryBlobs,
                    ct);

            string messageBody = SystemTemplateUtils.GetNewMessageBodyJson(
                command.MessageBody,
                command.Blobs
                    .Select(
                        x => new ValueTuple<string, string, string, ulong>
                        {
                            Item1 = x.FileName,
                            Item2 = x.HashAlgorithm,
                            Item3 = x.Hash,
                            Item4 = x.Size
                        })
                    .ToArray());

            string senderIdentifier =
                await this.IntegrationServiceMessagesSendQueryRepository.GetProfileIdentifierAsync(
                    sender.ProfileId,
                    ct);

            await using ITransaction messageTransaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            Message message = new(
                command.SentViaOperatorLoginId ?? command.SentViaLoginId,
                sender.ProfileId,
                new int[] { recipient.ProfileId },
                recipient.ProfileName,
                Template.SystemTemplateId,
                command.MessageSubject,
                this.RnuService.Get(),
                this.HandleJson(encryptor, messageBody),
                "{}",
                command.SentViaOperatorLoginId ?? command.SentViaLoginId,
                command.SentViaLoginId,
                encryptor.IV,
                messageAccessKeys,
                messageBlobs);

            await this.MessageAggregateRepository.AddAsync(message, ct);

            await this.UnitOfWork.SaveAsync(ct);

            (string messageSummaryXml, byte[] messageSummary, byte[] sendTimestamp) =
                await this.CreateMessageSummary(
                    message.MessageId,
                    message.Rnu,
                    sender.ProfileId,
                    command.SentViaLoginId,
                    command.SentViaOperatorLoginId,
                    profileKeys,
                    profileNames,
                    message.DateSent,
                    message.Subject,
                    messageBody,
                    message.MetaFields,
                    message.TemplateId,
                    decryptedTemporaryBlobs,
                    ct);

            message.UpdateExtendedSubject(
                messageSummaryXml,
                messageSummary,
                sendTimestamp);

            await this.UnitOfWork.SaveAsync(ct);

            NotificationMessages notificationMessages =
                await this.GetNotificationsAsync(
                    message.MessageId,
                    sender.ProfileName,
                    command.SendEvent,
                    ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.EmailQueueMessages,
                QueueMessageFeatures.Messages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.PushNotificationQueueMessages,
                QueueMessageFeatures.Messages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.ViberQueueMessages,
                QueueMessageFeatures.Messages,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await messageTransaction.CommitAsync(ct);

            // TODO: journal log with serviceoid
            return new(
                true,
                string.Empty,
                message.MessageId);
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
                await this.IntegrationServiceMessagesSendQueryRepository.GetTemporaryOrStorageBlobsAsync(
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
            string? rnu,
            int senderProfileId,
            int sentViaLoginId,
            int? sentViaOperatorLoginId,
            ProfileKeyVO[] allProfiles,
            GetProfileNamesVO[] allProfileNames,
            DateTime? dateSent,
            string subject,
            string? body,
            string? metaFields,
            int? templateId,
            DecryptedTemporaryBlobVO[] blobs,
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

            // TODO: move creating of MessageSummaryDO to Message.cs
            Message.MessageSummaryDO messageSummaryDO = new(
                messageId,
                rnu,
                new Message.MessageSummaryDO.MessageSummaryVOProfile(
                    senderProfileId,
                    senderProfileName,
                    sentViaLoginId,
                    sentViaOperatorLoginId),
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
                    .ToArray());

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

        private record NotificationMessages(
            EmailQueueMessage[] EmailQueueMessages,
            PushNotificationQueueMessage[] PushNotificationQueueMessages,
            ViberQueueMessage[] ViberQueueMessages);

        private async Task<NotificationMessages> GetNotificationsAsync(
            int messageId,
            string senderProfileName,
            string sendEvent,
            CancellationToken ct)
        {
            GetNotificationRecipientsVO[] notificationRecipients =
                await this.IntegrationServiceMessagesSendQueryRepository
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
                    QueueMessageFeatures.Messages,
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
                        Event = string.Format(NotificationEvent, sendEvent),
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId
                    }))
                .ToArray();

            PushNotificationQueueMessage[] pushNotificationRecipients = notificationRecipients
                .Where(e => !string.IsNullOrEmpty(e.PushNotificationUrl))
                .Select(e => new PushNotificationQueueMessage(
                    QueueMessageFeatures.Messages,
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
                .Where(e => e.IsPhoneNotificationEnabled)
                .Select(e => new ViberQueueMessage(
                    QueueMessageFeatures.Messages,
                    e.Phone,
                    string.Format(
                        viberBody,
                        webPortalUrl,
                        e.ProfileName.ToUpperInvariant(),
                        senderProfileName.ToUpperInvariant()),
                    new
                    {
                        Event = string.Format(NotificationEvent, sendEvent),
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId,
                        FallbackSmsBody = string.Format(
                            smsBody,
                            webPortalUrl,
                            ResourceHelper
                                .CyrillicToLatin(e.ProfileName)
                                .ToUpperInvariant(),
                            ResourceHelper
                                .CyrillicToLatin(senderProfileName)
                                .ToUpperInvariant()),
                    }))
                .ToArray();

            return new NotificationMessages(
                emailRecipients,
                pushNotificationRecipients,
                viberRecipients);
        }
    }
}
