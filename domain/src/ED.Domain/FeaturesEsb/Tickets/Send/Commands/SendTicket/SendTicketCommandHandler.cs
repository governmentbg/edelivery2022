using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using EnumsNET;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Options;
using static ED.Domain.IEsbTicketsSendQueryRepository;
using static ED.Domain.IProfilesService;

namespace ED.Domain
{
    internal record SendTicketCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        IAggregateRepository<Ticket> TicketAggregateRepository,
        IEncryptorFactory EncryptorFactory,
        Keystore.Keystore.KeystoreClient KeystoreClient,
        TimestampServiceClient TimestampServiceClient,
        IEsbTicketsSendQueryRepository EsbTicketsSendQueryRepository,
        IProfilesService ProfilesService,
        IRnuService RnuService,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<SendTicketCommand, int>
    {
        private const string NotificationEvent = "OnTicketSend";

        public async Task<int> Handle(
            SendTicketCommand command,
            CancellationToken ct)
        {
            // TODO: refactor
            using IEncryptor encryptor = this.EncryptorFactory.CreateEncryptor();

            int[] distinctRecipientProfileIds = new int[] { command.RecipientProfileId };

            int[] recipientProfileIds = new int[] { command.SenderProfileId };

            int[] allProfileIds = distinctRecipientProfileIds
                .Concat(recipientProfileIds)
                .Distinct() // in chase the sender is also among the recipients
                .ToArray();

            GetProfileNamesVO[] profileNames =
                await this.EsbTicketsSendQueryRepository.GetProfileNamesAsync(
                    allProfileIds,
                    ct);

            string[] recipientProfileNames =
                (from pn in profileNames
                 join rpId in recipientProfileIds
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
                    new int[] { command.Document.BlobId },
                    ct))
                .ToArray();

            Message.MessageBlobDO[] messageBlobs =
                await this.CreateMessageBlobsAsync(
                    profileKeys,
                    new int[] { command.Document.BlobId },
                    decryptedTemporaryBlobs,
                    ct);

            string messageBody = SystemTemplateUtils.GetTicketMessageBody(
                command.Body,
                command.Type.AsString(EnumFormat.Description) ?? throw new ArgumentException(""),
                command.DocumentSeries,
                command.DocumentNumber,
                command.IssueDate,
                command.VehicleNumber,
                command.ViolationDate,
                command.ViolatedProvision,
                command.PenaltyProvision,
                command.DueAmount,
                command.DiscountedPaymentAmount,
                command.IBAN,
                command.BIC,
                command.PaymentReason,
                (
                    command.Document.FileName,
                    command.Document.HashAlgorithm,
                    command.Document.Hash,
                    command.Document.Size));

            try
            {

                await using ITransaction transaction =
                    await this.UnitOfWork.BeginTransactionAsync(ct);

                Message message = new(
                    command.SenderLoginId,
                    command.SenderProfileId,
                    distinctRecipientProfileIds,
                    recipientsAsText,
                    command.TemplateId,
                    command.Subject,
                    this.RnuService.Get(),
                    this.HandleJson(encryptor, messageBody),
                    "{}",
                    command.SenderLoginId,
                    null,
                    encryptor.IV,
                    messageAccessKeys,
                    messageBlobs);

                await this.MessageAggregateRepository.AddAsync(message, ct);

                await this.UnitOfWork.SaveAsync(ct);

                (string messageSummaryXml, byte[] messageSummary, byte[] sendTimestamp) =
                    await this.CreateMessageSummary(
                        message.MessageId,
                        message.Rnu,
                        command.SenderProfileId,
                        profileKeys,
                        profileNames,
                        message.DateSent,
                        message.Subject,
                        messageBody,
                        message.MetaFields,
                        message.TemplateId,
                        decryptedTemporaryBlobs,
                        null,
                        null,
                        ct);

                message.UpdateExtendedSubject(
                    messageSummaryXml,
                    messageSummary,
                    sendTimestamp);

                await this.UnitOfWork.SaveAsync(ct);

                Ticket ticket = new(
                    message.MessageId,
                    command.Type.AsString(EnumFormat.Description) ?? throw new ArgumentException(""),
                    command.ViolationDate,
                    command.NotificationEmail,
                    command.NotificationPhone,
                    command.SenderProfileId,
                    command.DocumentSeries,
                    command.DocumentNumber,
                    command.IssueDate.Date,
                    command.RecipientIdentifier,
                    command.DocumentIdentifier,
                    command.SenderLoginId);

                await this.TicketAggregateRepository.AddAsync(ticket, ct);

                await this.UnitOfWork.SaveAsync(ct);

                NotificationMessages notificationMessages =
                    await this.GetNotificationsAsync(
                        command.IsRecipientIndividual,
                        distinctRecipientProfileIds.First(),
                        message.MessageId,
                        command.NotificationEmail,
                        command.NotificationPhone,
                        command.VehicleNumber,
                        command.ViolationDate,
                        command.Type,
                        command.DocumentSeries,
                        command.DocumentNumber,
                        command.DueAmount,
                        ct);

                await this.QueueMessagesService.PostMessagesAsync(
                    notificationMessages.EmailQueueMessages,
                    QueueMessageFeatures.Tickets,
                    ct);

                await this.QueueMessagesService.PostMessagesAsync(
                    notificationMessages.ViberQueueMessages,
                    QueueMessageFeatures.Tickets,
                    ct);

                await this.UnitOfWork.SaveAsync(ct);

                await transaction.CommitAsync(ct);

                return message.MessageId;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                int? messageId =
                    await this.EsbTicketsSendQueryRepository.GetDuplicateTicketIdAsync(
                        command.SenderProfileId,
                        command.DocumentSeries,
                        command.DocumentNumber,
                        command.IssueDate.Date,
                        ct);

                if (messageId.HasValue)
                {
                    return messageId.Value;
                }

                throw;
            }
            catch
            {
                throw;
            }
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
                await this.EsbTicketsSendQueryRepository.GetTemporaryOrStorageBlobsAsync(
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
                rnu,
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

        private record NotificationMessages(
            EmailQueueMessage[] EmailQueueMessages,
            ViberQueueMessage[] ViberQueueMessages);

        private async Task<NotificationMessages> GetNotificationsAsync(
            bool isRecipientIndividual,
            int recipientProfileId,
            int messageId,
            string? notificationEmail,
            string? notificationPhone,
            string vehicleNumber,
            DateTime violationDate,
            TicketType ticketType,
            string? documentSeries,
            string documentNumber,
            string dueAmount,
            CancellationToken ct)
        {
            GetNotificationRecipientsVO[] notificationRecipients =
                await this.EsbTicketsSendQueryRepository
                    .GetNotificationRecipientsAsync(
                        isRecipientIndividual,
                        messageId,
                        ct);

            if (isRecipientIndividual)
            {
                // if individual is
                // - activated
                // - has checked email or sms/viber notficaition ON
                // => then omit notification contacts!
                bool isIndividualActivated =
                    await this.EsbTicketsSendQueryRepository.IsIndividualProfileActivatedAsync(
                        recipientProfileId,
                        ct);

                GetNotificationRecipientsVO tmp = notificationRecipients.First();

                bool skipNotificationContacts = isIndividualActivated
                    && (tmp.IsEmailNotificationEnabled || tmp.IsPhoneNotificationEnabled);

                if (!skipNotificationContacts)
                {
                    notificationRecipients =
                        notificationRecipients
                            .Concat(new GetNotificationRecipientsVO[]
                            {
                                new(
                                    recipientProfileId,
                                    null,
                                    !string.IsNullOrEmpty(notificationEmail),
                                    notificationEmail ?? string.Empty,
                                    !string.IsNullOrEmpty(notificationPhone),
                                    notificationPhone ?? string.Empty)
                            })
                            .ToArray();
                }
            }
            else
            {
                // if recipient is
                // - legal entity
                // => then omit notification contacts, and send only to legal entity's email!
                GetProfileContactsVO legalEntityContacts =
                    await this.EsbTicketsSendQueryRepository.GetProfileContactsAsync(
                        recipientProfileId,
                        ct);

                notificationRecipients =
                    notificationRecipients
                        .Concat(new GetNotificationRecipientsVO[]
                        {
                            new(
                                recipientProfileId,
                                null,
                                !string.IsNullOrEmpty(legalEntityContacts.Email),
                                legalEntityContacts.Email,
                                false,
                                legalEntityContacts.Phone)
                        })
                        .ToArray();
            }

            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.SentTicketEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.SentTicketEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.SentTicketEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.SentTicketEmailBody)}");

            string smsBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.SentTicketSMSBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.SentTicketSMSBody)}");

            string viberBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.SentTicketViberBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.SentTicketViberBody)}");

            string webPortalUrl =
                this.DomainOptionsAccessor.Value.WebPortalUrl
                ?? throw new Exception(
                    $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

            string q = $"p={recipientProfileId}&t={messageId}".ToUrlSafeBase64();

            string returnUrl = HttpUtility.UrlEncode($"/Tickets/Distribute?q={q}");
            string webPortalUrlShorts = $"{webPortalUrl}/s?returnUrl={returnUrl}";

            string webPortalUrlShortsLoadObligation = $"{webPortalUrl}/o?q={q}";

            EmailQueueMessage[] emailRecipients = notificationRecipients
                .Where(e => !string.IsNullOrEmpty(e.Email))
                .Where(e => e.IsEmailNotificationEnabled)
                .DistinctBy(e => e.Email.ToLower())
                .Select(e => new EmailQueueMessage(
                    QueueMessageFeatures.Tickets,
                    e.Email.ToLower(),
                    emailSubect,
                    string.Format(
                        emailBody,
                        vehicleNumber,
                        violationDate.ToString("dd.MM.yyyy"),
                        ticketType.AsString(EnumFormat.Description) ?? throw new ArgumentException(""),
                        ticketType == TicketType.Ticket ? documentSeries : string.Empty,
                        documentNumber,
                        dueAmount,
                        webPortalUrlShorts,
                        webPortalUrlShortsLoadObligation),
                    true,
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId
                    }))
                .ToArray();

            ViberQueueMessage[] viberRecipients = notificationRecipients
                .Where(e => !string.IsNullOrEmpty(e.Phone))
                .Where(e => e.IsPhoneNotificationEnabled)
                .DistinctBy(e => e.Phone.ToPhone())
                .Select(e => new ViberQueueMessage(
                    QueueMessageFeatures.Tickets,
                    e.Phone.ToPhone(),
                    string.Format(
                        viberBody,
                        vehicleNumber,
                        violationDate.ToString("dd.MM.yyyy"),
                        ticketType.AsString(EnumFormat.Description) ?? throw new ArgumentException(""),
                        ticketType == TicketType.Ticket ? documentSeries : string.Empty,
                        documentNumber,
                        dueAmount,
                        webPortalUrlShorts,
                        webPortalUrlShortsLoadObligation),
                    new
                    {
                        Event = NotificationEvent,
                        MessageId = messageId,
                        RecipientProfileId = e.ProfileId,
                        RecipientLoginId = e.LoginId,
                        FallbackSmsBody = string.Format(
                            smsBody,
                            vehicleNumber,
                            ticketType == TicketType.Ticket ? $"{documentSeries}{documentNumber}" : documentNumber,
                            dueAmount),
                    }))
                .ToArray();

            return new NotificationMessages(
                emailRecipients,
                viberRecipients);
        }
    }
}
