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
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;
using static ED.Domain.ICodeMessageSendQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    internal record SendCodeMessageCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IEncryptorFactory EncryptorFactory,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        TimestampServiceClient TimestampServiceClient,
        ICodeMessageSendQueryRepository CodeMessageSendQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IProfilesService ProfilesService,
        IRnuService RnuService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<SendCodeMessageCommand, Unit>
    {
        private const string NotificationEvent = "OnSend";

        public async Task<Unit> Handle(
            SendCodeMessageCommand command,
            CancellationToken ct)
        {
            // TODO: refactor
            using IEncryptor encryptor = this.EncryptorFactory.CreateEncryptor();

            ICodeMessageSendQueryRepository.GetExistingIndividualVO? existingIndividual =
               await this.CodeMessageSendQueryRepository.GetExistingIndividualAsync(
                   command.Identifier,
                   ct);

            int recipientProfileId;
            if (existingIndividual == null)
            {
                // create the key upfront to minimize the transaction lifetime
                Keystore.CreateRsaKeyResponse newRsaKey =
                    await this.KeystoreClient.CreateRsaKeyAsync(
                        request: new Empty(),
                        cancellationToken: ct);

                DateTime issuedAt = DateTime.Now;
                DateTime expiresAt =
                    issuedAt.Add(this.DomainOptionsAccessor.Value.ProfileKeyExpiration);

                await using ITransaction profileTransaction =
                    await this.UnitOfWork.BeginTransactionAsync(ct);

                Profile profile = new(
                    command.FirstName,
                    command.MiddleName,
                    command.LastName,
                    command.Identifier,
                    command.Phone,
                    command.Email,
                    string.Empty,
                    command.SenderLoginId,
                    newRsaKey.Key.Provider,
                    newRsaKey.Key.KeyName,
                    newRsaKey.Key.OaepPadding,
                    issuedAt,
                    expiresAt);

                await this.ProfileAggregateRepository.AddAsync(profile, ct);

                await this.UnitOfWork.SaveAsync(ct);

                await this.TargetGroupProfileAggregateRepository.AddAsync(
                    new TargetGroupProfile(
                        TargetGroup.IndividualTargetGroupId,
                        profile.Id),
                    ct);

                await this.UnitOfWork.SaveAsync(ct);

                await profileTransaction.CommitAsync(ct);

                recipientProfileId = profile.Id;
            }
            else
            {
                recipientProfileId = existingIndividual.ProfileId;
            }

            ProfileKeyVO recipientProfileKey =
                await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                    recipientProfileId,
                    ct);

            ProfileKeyVO senderProfileKey =
                await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                    command.SenderProfileId,
                    ct);

            Message.MessageAccessKeyDO[] messageAccessKeys =
                await this.CreateMessageAccessKeysAsync(
                    new[] { recipientProfileKey, senderProfileKey },
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
                    new[] { recipientProfileKey, senderProfileKey },
                    command.BlobIds,
                    decryptedTemporaryBlobs,
                    ct);

            await using ITransaction messageTransaction =
                    await this.UnitOfWork.BeginTransactionAsync(ct);

            Message message = new(
                command.SenderLoginId,
                command.SenderProfileId,
                recipientProfileId,
                this.GenerateAccessCode(),
                command.FirstName,
                command.MiddleName,
                command.LastName,
                command.Phone,
                command.Email,
                command.TemplateId,
                command.Subject,
                this.RnuService.Get(),
                this.HandleJson(encryptor, command.Body),
                command.MetaFields,
                command.CreatedBy,
                encryptor.IV,
                messageAccessKeys,
                messageBlobs);

            await this.MessageAggregateRepository.AddAsync(message, ct);

            await this.UnitOfWork.SaveAsync(ct);

            GetProfileNamesVO[] profileNames =
                await this.CodeMessageSendQueryRepository.GetProfileNamesAsync(
                    new[] { command.SenderProfileId, recipientProfileId },
                    ct);

            (string messageSummaryXml, byte[] messageSummary, byte[] sendTimestamp) =
                await this.CreateMessageSummary(
                    message.MessageId,
                    message.Rnu,
                    command.SenderProfileId,
                    new[] { recipientProfileKey, senderProfileKey },
                    profileNames,
                    message.DateSent,
                    message.Subject,
                    command.Body,
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
                    message.AccessCode!.AccessCode,
                    profileNames.Single(pn => pn.ProfileId == command.SenderProfileId).ProfileName,
                    ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.EmailQueueMessages,
                QueueMessageFeatures.Messages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.SmsQueueMessages,
                QueueMessageFeatures.Messages,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await messageTransaction.CommitAsync(ct);

            return default;
        }

        // TODO: improve security
        private Guid GenerateAccessCode()
        {
            return Guid.NewGuid();
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
                await this.CodeMessageSendQueryRepository.GetTemporaryOrStorageBlobsAsync(
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
                .Single(e => e.ProfileId == senderProfileId);

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
                rnu,
                new Message.MessageSummaryDO.MessageSummaryVOProfile(
                    senderProfileKey.ProfileId,
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

        private const string OpenCodeMessagePath = "/CodeMessages";

        private record NotificationMessages(
            EmailQueueMessage[] EmailQueueMessages,
            SmsQueueMessage[] SmsQueueMessages);

        private async Task<NotificationMessages> GetNotificationsAsync(
            int messageId,
            Guid accessCode,
            string senderProfileName,
            CancellationToken ct)
        {
            GetNotificationRecipientsVO[] notificationRecipients =
                await this.CodeMessageSendQueryRepository
                .GetNotificationRecipientsAsync(
                    messageId,
                    ct);

            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedCodeMessageEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedCodeMessageEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedCodeMessageEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedCodeMessageEmailBody)}");

            string smsBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.ReceivedCodeMessageSMSBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.ReceivedCodeMessageSMSBody)}");

            string webPortalUrl =
                this.DomainOptionsAccessor.Value.WebPortalUrl
                ?? throw new Exception(
                    $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

            EmailQueueMessage[] emailRecipients = notificationRecipients
                .Select(e => new EmailQueueMessage(
                    QueueMessageFeatures.Messages,
                    e.Email,
                    emailSubect,
                    string.Format(
                        emailBody,
                        e.ProfileName,
                        senderProfileName,
                        $"{webPortalUrl}{OpenCodeMessagePath}",
                        accessCode),
                    false,
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId
                    }))
                .ToArray();

            SmsQueueMessage[] smsRecipients = notificationRecipients
                .Select(e => new SmsQueueMessage(
                    QueueMessageFeatures.Messages,
                    e.Phone,
                    string.Format(
                        smsBody,
                        $"{webPortalUrl}{OpenCodeMessagePath}",
                        accessCode,
                        ResourceHelper
                            .CyrillicToLatin(senderProfileName)
                            .ToUpperInvariant()),
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                    }))
                .ToArray();

            return new NotificationMessages(emailRecipients, smsRecipients);
        }
    }
}
