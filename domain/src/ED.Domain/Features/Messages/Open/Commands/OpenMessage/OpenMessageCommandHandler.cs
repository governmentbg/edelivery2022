using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using MediatR;
using Microsoft.Extensions.Options;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    internal record OpenMessageCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Message> MessageAggregateRepository,
        TimestampServiceClient TimestampServiceClient,
        IMessageOpenQueryRepository MessageOpenQueryRepository,
        IQueueMessagesService QueueMessagesService,
        EncryptorFactoryV1 EncryptorFactoryV1,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<OpenMessageCommand, int>
    {
        private const string NotificationEvent = "OnDeliver";

        public async Task<int> Handle(
            OpenMessageCommand command,
            CancellationToken ct)
        {
            Message message = await this.MessageAggregateRepository
                .FindAsync(command.MessageId, ct);

            if (message.IsAlreadyOpen(command.ProfileId))
            {
                return message.MessageId;
            }

            DateTime now = DateTime.Now;

            byte[]? recipientMessageSummary;
            string? recipientMessageSummaryXml = null;

            byte[] messageSummarySha256;
            if (message.MessageSummaryVersion == MessageSummaryVersion.V1)
            {
                using IEncryptor encryptorV1 = this.EncryptorFactoryV1.CreateEncryptor();
                messageSummarySha256 = SHA256.HashData(
                    encryptorV1.Decrypt(
                        message.MessageSummary
                        ?? throw new Exception("MessageSummary should not be null")));

                recipientMessageSummary = message.MessageSummary;
            }
            else if (message.MessageSummaryVersion == MessageSummaryVersion.V2)
            {
                using MemoryStream memoryStream = new(
                    message.MessageSummary
                    ?? throw new Exception("MessageSummary should not be null"));
                messageSummarySha256 = XmlCanonicalizationHelper.GetSha256Hash(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);

                XmlSerializer serializer = new(typeof(Message.MessageSummaryDO));
                using XmlReader reader = XmlReader.Create(memoryStream);

                Message.MessageSummaryDO messageSummaryDO =
                    (Message.MessageSummaryDO?)serializer.Deserialize(reader)
                        ?? throw new Exception("MessageSummaryXml should not be null");

                messageSummaryDO.DateReceived = now;
                messageSummaryDO.Recipients = messageSummaryDO.Recipients
                    .Where(e => e.ProfileId == command.ProfileId)
                    .ToArray();

                // TODO: if message are "transferred" from one profile to another
                if (!messageSummaryDO.Recipients.Any())
                {
                    string profileName =
                        await this.MessageOpenQueryRepository.GetProfileNameAsync(
                            command.ProfileId,
                            ct);

                    messageSummaryDO.Recipients =
                        new Message.MessageSummaryDO.MessageSummaryVOProfile[]
                        {
                            new Message.MessageSummaryDO.MessageSummaryVOProfile(
                                command.ProfileId,
                                profileName),
                        };
                }

                using MemoryStream xmlMemoryStream = new();
                using XmlWriter xmlWriter = XmlWriter.Create(
                    xmlMemoryStream,
                    new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true,
                        Encoding = Encoding.UTF8
                    });

                serializer.Serialize(xmlWriter, messageSummaryDO);

                recipientMessageSummaryXml =
                    Encoding.UTF8.GetString(xmlMemoryStream.ToArray());
                recipientMessageSummary = Encoding.UTF8.GetBytes(recipientMessageSummaryXml);
            }
            else
            {
                throw new Exception($"Unknown MessageSummaryVersion {message.MessageSummaryVersion}");
            }

            byte[] timestamp = await this.TimestampServiceClient.SubmitAsync(
                message.MessageId,
                EncryptionHelper.Sha256Algorithm,
                messageSummarySha256,
                ct);

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            message.UpdateAsOpen(
                command.ProfileId,
                command.LoginId,
                now,
                timestamp,
                recipientMessageSummary,
                recipientMessageSummaryXml);

            await this.UnitOfWork.SaveAsync(ct);

            NotificationMessages notificationMessages =
               await this.GetNotificationsAsync(
                    message.MessageId,
                    message.Subject,
                    command.ProfileId,
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

        private record NotificationMessages(
            EmailQueueMessage[] EmailQueueMessages,
            SmsQueueMessage[] SmsQueueMessages,
            PushNotificationQueueMessage[] PushNotificationQueueMessages,
            ViberQueueMessage[] ViberQueueMessages);

        private async Task<NotificationMessages> GetNotificationsAsync(
            int messageId,
            string messageSubject,
            int recipientProfileId,
            CancellationToken ct)
        {
            string recipientProfileName =
                await this.MessageOpenQueryRepository.GetProfileNameAsync(
                    recipientProfileId,
                    ct);

            GetNotificationRecipientsOnDeliveryVO[] notificationRecipients =
                await this.MessageOpenQueryRepository
                    .GetNotificationRecipientsOnDeliveryAsync(
                        messageId,
                        ct);

            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.DeliveredEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.DeliveredEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.DeliveredEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.DeliveredEmailBody)}");

            string smsBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.DeliveredSMSBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.DeliveredSMSBody)}");

            string viberBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.DeliveredViberBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.DeliveredViberBody)}");

            string webPortalUrl =
                this.DomainOptionsAccessor.Value.WebPortalUrl
                ?? throw new Exception(
                    $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

            EmailQueueMessage[] emailRecipients = notificationRecipients
                .Where(e => e.IsEmailNotificationOnDeliveryEnabled)
                .Select(e => new EmailQueueMessage(
                    e.Email,
                    emailSubect,
                    string.Format(
                        emailBody,
                        e.LoginName,
                        e.ProfileName,
                        messageSubject,
                        recipientProfileName,
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
                .Where(e => e.IsSmsNotificationOnDeliveryEnabled)
                .Select(e => new SmsQueueMessage(
                    e.Phone,
                    string.Format(
                        smsBody,
                        webPortalUrl,
                        ResourceHelper
                            .CyrillicToLatin(recipientProfileName)
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
                        EventType = "Delivered",
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
                .Where(e => e.IsViberNotificationOnDeliveryEnabled)
                .Select(e => new ViberQueueMessage(
                    e.Phone,
                    string.Format(
                        viberBody,
                        webPortalUrl,
                        recipientProfileName.ToUpperInvariant()),
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
