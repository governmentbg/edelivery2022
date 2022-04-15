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
using static ED.Domain.IIntegrationServiceCodeMessagesSendQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    internal record SendMessage1WithAccessCodeCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        BlobsServiceClient BlobsServiceClient,
        IEncryptorFactory EncryptorFactory,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        TimestampServiceClient TimestampServiceClient,
        OrnServiceClient OrnServiceClient,
        IIntegrationServiceCodeMessagesSendQueryRepository IntegrationServiceCodeMessagesSendQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IProfilesService ProfilesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<SendMessage1WithAccessCodeCommand, SendMessage1WithAccessCodeCommandResult>
    {
        private const string NotificationEvent = "IntegrationService:{0}:OnSend";

        public async Task<SendMessage1WithAccessCodeCommandResult> Handle(
            SendMessage1WithAccessCodeCommand command,
            CancellationToken ct)
        {
            bool checkSendMessageWithAccessCode =
                await this.IntegrationServiceCodeMessagesSendQueryRepository
                    .CheckSendMessageWithAccessCodeAsync(
                        command.SenderProfileId,
                        ct);

            if (!checkSendMessageWithAccessCode)
            {
                return new(
                    false,
                    "The profile is not allowed to send messages with access code!",
                    null);
            }

            using IEncryptor encryptor = this.EncryptorFactory.CreateEncryptor();

            GetExistingIndividualVO? existingIndividual =
               await this.IntegrationServiceCodeMessagesSendQueryRepository.GetExistingIndividualAsync(
                   command.RecipientIdentifier,
                   ct);

            int? ProfileId = null;

            if (existingIndividual == null)
            {
                string? validationError = this.ValidateProfileData(
                    command.RecipientIdentifier,
                    command.RecipientFirstName,
                    command.RecipientMiddleName,
                    command.RecipientLastName,
                    command.RecipientEmail);

                if (!string.IsNullOrWhiteSpace(validationError))
                {
                    return new(false, validationError, null);
                }

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
                    command.RecipientFirstName,
                    command.RecipientMiddleName,
                    command.RecipientLastName,
                    command.RecipientIdentifier,
                    command.RecipientPhone,
                    command.RecipientEmail,
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

                ProfileId = profile.Id;
            }
            else
            {
                ProfileId = existingIndividual.ProfileId;
            }

            int[] blobIds = new int[command.Documents.Length];

            (string, string, string, ulong)[] blobAttributes =
                new (string, string, string, ulong)[command.Documents.Length];

            for (int i = 0; i < blobIds.Length; i++)
            {
                BlobsServiceClient.UploadBlobVO uploadedBlob =
                    await this.BlobsServiceClient.UploadProfileBlobAsync(
                        command.Documents[i].FileName,
                        command.Documents[i].FileContent.AsMemory(0, command.Documents[i].FileContent.Length),
                        command.SenderProfileId,
                        command.SenderLoginId,
                        ProfileBlobAccessKeyType.Temporary,
                        command.Documents[i].DocumentRegistrationNumber,
                        ct);

                blobIds[i] = uploadedBlob.BlobId!.Value;

                blobAttributes[i] =
                    (
                        command.Documents[i].FileName,
                        uploadedBlob.HashAlgorithm ?? string.Empty,
                        uploadedBlob.Hash ?? string.Empty,
                        Convert.ToUInt64(command.Documents[i].FileContent.Length)
                    );
            }

            int[] allProfileIds = new int[]
           {
                ProfileId.Value,
                command.SenderProfileId
           };

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
                blobAttributes);

            string senderIdentifier =
                await this.IntegrationServiceCodeMessagesSendQueryRepository.GetProfileIdentifierAsync(
                    command.SenderProfileId,
                    ct);

            string orn =
                await this.OrnServiceClient.SubmitAsync(senderIdentifier, ct);

            await using ITransaction messageTransaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            Message message = new(
                command.SenderLoginId,
                command.SenderProfileId,
                ProfileId!.Value,
                this.GenerateAccessCode(),
                command.RecipientFirstName,
                command.RecipientMiddleName,
                command.RecipientLastName,
                command.RecipientPhone,
                command.RecipientEmail,
                Template.SystemTemplateId,
                command.MessageSubject,
                orn,
                null,
                null,
                this.HandleJson(encryptor, messageBody),
                "{}",
                command.SenderLoginId,
                encryptor.IV,
                messageAccessKeys,
                messageBlobs);

            await this.MessageAggregateRepository.AddAsync(message, ct);

            await this.UnitOfWork.SaveAsync(ct);

            GetProfileNamesVO[] profileNames =
               await this.IntegrationServiceCodeMessagesSendQueryRepository
                    .GetProfileNamesAsync(
                        new[] {
                            command.SenderProfileId,
                            ProfileId!.Value
                        },
                        ct);

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
                    message.AccessCode!.AccessCode,
                    profileNames.Single(pn => pn.ProfileId == command.SenderProfileId).ProfileName,
                    command.SendEvent,
                    ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.EmailQueueMessages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.SmsQueueMessages,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await messageTransaction.CommitAsync(ct);

            // TODO: journal log with serviceoid
            return new(
                true,
                string.Empty,
                message.MessageId);
        }

        private string? ValidateProfileData(
            string identifier,
            string firstName,
            string middleName,
            string lastName,
            string email)
        {
            if (!ProfileValidationUtils.IsValidEGN(identifier)
                && !ProfileValidationUtils.IsValidLNC(identifier))
            {
                return "Invalid identifier";
            }

            if (string.IsNullOrEmpty(firstName)
                || !ProfileValidationUtils.IsValidName(firstName))
            {
                return "Invalid first name";
            }

            if (string.IsNullOrEmpty(middleName)
                || !ProfileValidationUtils.IsValidName(middleName))
            {
                return "Invalid middle name";
            }

            if (string.IsNullOrEmpty(lastName)
                || !ProfileValidationUtils.IsValidName(lastName))
            {
                return "Invalid last name";
            }

            if (string.IsNullOrEmpty(email)
                || !ProfileValidationUtils.IsValidEmail(email))
            {
                return "Invalid Email";
            }

            return null;
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
                await this.IntegrationServiceCodeMessagesSendQueryRepository.GetTemporaryOrStorageBlobsAsync(
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
                orn,
                referencedOrn,
                additionalIdentifier,
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

        private const string OpenCodeMessagePath = "/CodeMessages/Open";

        private record NotificationMessages(
            EmailQueueMessage[] EmailQueueMessages,
            SmsQueueMessage[] SmsQueueMessages);

        private async Task<NotificationMessages> GetNotificationsAsync(
            int messageId,
            Guid accessCode,
            string senderProfileName,
            string sendEvent,
            CancellationToken ct)
        {
            GetNotificationRecipientsVO[] notificationRecipients =
                await this.IntegrationServiceCodeMessagesSendQueryRepository
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
                        Event = string.Format(NotificationEvent, sendEvent),
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId
                    }))
                .ToArray();

            SmsQueueMessage[] smsRecipients = notificationRecipients
                .Select(e => new SmsQueueMessage(
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
                        Event = string.Format(NotificationEvent, sendEvent),
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                    }))
                .ToArray();

            return new NotificationMessages(emailRecipients, smsRecipients);
        }
    }
}
