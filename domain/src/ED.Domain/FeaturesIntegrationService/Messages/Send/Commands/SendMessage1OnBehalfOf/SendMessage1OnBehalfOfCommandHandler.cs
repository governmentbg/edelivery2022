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
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    internal record SendMessage1OnBehalfOfCommandHandler(
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
        : IRequestHandler<SendMessage1OnBehalfOfCommand, SendMessage1OnBehalfOfCommandResult>
    {
        private const string NotificationEvent = "IntegrationService:{0}:OnSend";

        public async Task<SendMessage1OnBehalfOfCommandResult> Handle(
            SendMessage1OnBehalfOfCommand command,
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

            int[] recipientTargetGroupSearch;

            if (command.RecipientTargetGroupId == TargetGroup.PublicAdministrationTargetGroupId)
            {
                recipientTargetGroupSearch = new int[]
                {
                    TargetGroup.PublicAdministrationTargetGroupId,
                    TargetGroup.SocialOrganizationTargetGroupId,
                };
            }
            else
            {
                recipientTargetGroupSearch = new int[] { command.RecipientTargetGroupId };
            }

            GetRecipientOrSenderProfileVO? recipient =
                await this.IntegrationServiceMessagesSendQueryRepository
                    .GetRecipientOrSenderProfileAsync(
                        command.RecipientIdentifier,
                        recipientTargetGroupSearch,
                        ct);

            if (recipient == null)
            {
                return new(
                    false,
                    "The requested receiver profile does not exist or is deactivated!", // copied from old implementation
                    null);
            }

            int[] senderTargetGroupSearch;

            if (command.SenderTargetGroupId == TargetGroup.PublicAdministrationTargetGroupId)
            {
                senderTargetGroupSearch = new int[]
                {
                    TargetGroup.PublicAdministrationTargetGroupId,
                    TargetGroup.SocialOrganizationTargetGroupId,
                };
            }
            else
            {
                senderTargetGroupSearch = new int[] { command.SenderTargetGroupId };
            }

            GetRecipientOrSenderProfileVO? sender =
                await this.IntegrationServiceMessagesSendQueryRepository
                    .GetRecipientOrSenderProfileAsync(
                        command.SenderIdentifier,
                        senderTargetGroupSearch,
                        ct);

            if (sender == null
               && command.SenderTargetGroupId != TargetGroup.IndividualTargetGroupId)
            {
                return new(
                    false,
                    "Can't create sender profile!",
                    null);
            }

            (int? SenderProfileId, string? SenderProfileName) = (null, null);

            if (sender == null)
            {
                string? validationError = this.ValidateProfileData(
                    command.SenderIdentifier,
                    command.SenderFirstName,
                    command.SenderLastName,
                    command.SenderEmail);

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
                    command.SenderFirstName
                        ?? command.SenderEmail
                            ?? command.SenderIdentifier,
                    string.Empty,
                    command.SenderLastName ?? string.Empty,
                    command.SenderIdentifier,
                    command.SenderPhone ?? string.Empty,
                    command.SenderEmail!,
                    string.Empty,
                    Login.SystemLoginId,
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

                SenderProfileId = profile.Id;
                SenderProfileName = profile.ElectronicSubjectName;
            }
            else
            {
                SenderProfileId = sender.ProfileId;
                SenderProfileName = sender.ProfileName;
            }

            using IEncryptor encryptor = this.EncryptorFactory.CreateEncryptor();

            GetProfileNamesVO[] profileNames =
                (await this.IntegrationServiceMessagesSendQueryRepository
                    .GetProfileNamesAsync(
                        new[] { recipient.ProfileId },
                    ct))
                    .Concat(new GetProfileNamesVO[]
                    {
                        new GetProfileNamesVO(SenderProfileId.Value, SenderProfileName)
                    })
                    .ToArray();

            int[] allProfileIds = new int[]
            {
                SenderProfileId.Value,
                recipient.ProfileId
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
                .First(e => e.ProfileId == SenderProfileId.Value);

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
                    SenderProfileId.Value,
                    ct);

            await using ITransaction messageTransaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            Message message = new(
                command.SentViaOperatorLoginId ?? command.SentViaLoginId,
                SenderProfileId.Value,
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
                    SenderProfileId.Value,
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
                    profileNames.Single(pn => pn.ProfileId == SenderProfileId.Value).ProfileName,
                    command.SendEvent,
                    ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.EmailQueueMessages,
                QueueMessageFeatures.Messages,
                ct);

            await this.QueueMessagesService.PostMessagesAsync(
                notificationMessages.SmsQueueMessages,
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

        private string? ValidateProfileData(
            string identifier,
            string? firstName,
            string? lastName,
            string? email)
        {
            if (!ProfileValidationUtils.IsValidEGN(identifier)
                && !ProfileValidationUtils.IsValidLNC(identifier))
            {
                return "Invalid identifier";
            }

            if (!ProfileValidationUtils.IsValidName(firstName))
            {
                return "Invalid first name";
            }

            if (!ProfileValidationUtils.IsValidName(lastName))
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
            SmsQueueMessage[] SmsQueueMessages,
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

            SmsQueueMessage[] smsRecipients = notificationRecipients
                .Where(e => e.IsSmsNotificationEnabled)
                .Select(e => new SmsQueueMessage(
                    QueueMessageFeatures.Messages,
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
                .Where(e => e.IsViberNotificationEnabled)
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
