//using System;
//using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
//using static ED.Domain.IEsbTicketsSendQueryRepository;

namespace ED.Domain
{
    internal record AnnulTicketCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Ticket> TicketAggregateRepository,
        IEsbTicketsSendQueryRepository EsbTicketsSendQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<AnnulTicketCommand, Unit>
    {
        //private const string NotificationEvent = "OnTicketAnnulled";

        public async Task<Unit> Handle(
            AnnulTicketCommand command,
            CancellationToken ct)
        {
            Ticket ticket =
                await this.TicketAggregateRepository.FindAsync(command.TicketId, ct);

            //bool isTicketRecipientIndividual =
            //    await this.EsbTicketsSendQueryRepository.IsRecipientProfileIndividualAsync(
            //        command.TicketId,
            //        ct);

            //await using ITransaction transaction =
            //    await this.UnitOfWork.BeginTransactionAsync(ct);

            ticket.Annul(
                command.AnnulDate,
                command.AnnulmentReason,
                command.ActionLoginId);

            await this.UnitOfWork.SaveAsync(ct);

            //NotificationMessages notificationMessages =
            //    await this.GetNotificationsAsync(
            //        isTicketRecipientIndividual,
            //        command.TicketId,
            //        ticket.Email,
            //        ticket.Phone,
            //        ct);

            //await this.QueueMessagesService.PostMessagesAsync(
            //    notificationMessages.EmailQueueMessages,
            //    QueueMessageFeatures.Tickets,
            //    ct);

            //await this.QueueMessagesService.PostMessagesAsync(
            //    notificationMessages.ViberQueueMessages,
            //    QueueMessageFeatures.Tickets,
            //    ct);

            //await this.UnitOfWork.SaveAsync(ct);

            //await transaction.CommitAsync(ct);

            return default;
        }

        //private record NotificationMessages(
        //    EmailQueueMessage[] EmailQueueMessages,
        //    ViberQueueMessage[] ViberQueueMessages);

        //private async Task<NotificationMessages> GetNotificationsAsync(
        //    bool isRecipientIndividual,
        //    int messageId,
        //    string? notificationEmail,
        //    string? notificationPhone,
        //    CancellationToken ct)
        //{
        //    GetNotificationRecipientsVO[] notificationRecipients =
        //        await this.EsbTicketsSendQueryRepository
        //            .GetNotificationRecipientsAsync(
        //                isRecipientIndividual,
        //                messageId,
        //                ct);

        //    GetNotificationRecipientsVO? tmp = notificationRecipients.FirstOrDefault();

        //    if (tmp == null)
        //    {
        //        return new NotificationMessages(
        //            Array.Empty<EmailQueueMessage>(),
        //            Array.Empty<ViberQueueMessage>());
        //    }

        //    notificationRecipients =
        //        notificationRecipients
        //            .Concat(new GetNotificationRecipientsVO[]
        //            {
        //                new GetNotificationRecipientsVO(
        //                    tmp.ProfileId,
        //                    tmp.ProfileName,
        //                    tmp.LoginId,
        //                    !string.IsNullOrEmpty(notificationEmail),
        //                    notificationEmail ?? string.Empty,
        //                    !string.IsNullOrEmpty(notificationPhone),
        //                    !string.IsNullOrEmpty(notificationPhone),
        //                    notificationPhone ?? string.Empty)
        //            })
        //            .ToArray();

        //    string emailSubect = Notifications.ResourceManager
        //        .GetString(nameof(Notifications.AnnulledTicketEmailSubject))
        //            ?? throw new Exception(
        //                $"Missing resource {nameof(Notifications.AnnulledTicketEmailSubject)}");

        //    string emailBody = Notifications.ResourceManager
        //        .GetString(nameof(Notifications.AnnulledTicketEmailBody))
        //            ?? throw new Exception(
        //                $"Missing resource {nameof(Notifications.AnnulledTicketEmailBody)}");

        //    string smsBody = Notifications.ResourceManager
        //        .GetString(nameof(Notifications.AnnulledTicketSMSBody))
        //            ?? throw new Exception(
        //                $"Missing resource {nameof(Notifications.AnnulledTicketSMSBody)}");

        //    string viberBody = Notifications.ResourceManager
        //        .GetString(nameof(Notifications.AnnulledTicketViberBody))
        //            ?? throw new Exception(
        //                $"Missing resource {nameof(Notifications.AnnulledTicketViberBody)}");

        //    string webPortalUrl =
        //        this.DomainOptionsAccessor.Value.WebPortalUrl
        //        ?? throw new Exception(
        //            $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

        //    EmailQueueMessage[] emailRecipients = notificationRecipients
        //        .Where(e => !string.IsNullOrEmpty(e.Email))
        //        .Where(e => e.IsEmailNotificationEnabled)
        //        .DistinctBy(e => e.Email)
        //        .Select(e => new EmailQueueMessage(
        //            QueueMessageFeatures.Tickets,
        //            e.Email,
        //            emailSubect,
        //            emailBody,
        //            false,
        //            new
        //            {
        //                Event = NotificationEvent,
        //                MessageId = messageId,
        //                RecipientProfileId = e.ProfileId,
        //                RecipientLoginId = e.LoginId
        //            }))
        //        .ToArray();

        //    ViberQueueMessage[] viberRecipients = notificationRecipients
        //        .Where(e => !string.IsNullOrEmpty(e.Phone))
        //        .Where(e => e.IsViberNotificationEnabled || e.IsSmsNotificationEnabled)
        //        .DistinctBy(e => e.Phone)
        //        .Select(e => new ViberQueueMessage(
        //            QueueMessageFeatures.Tickets,
        //            e.Phone,
        //            viberBody,
        //            new
        //            {
        //                Event = NotificationEvent,
        //                MessageId = messageId,
        //                RecipientProfileId = e.ProfileId,
        //                RecipientLoginId = e.LoginId,
        //                FallbackSmsBody = smsBody,
        //            }))
        //        .ToArray();

        //    return new NotificationMessages(
        //        emailRecipients,
        //        viberRecipients);
        //}
    }
}
