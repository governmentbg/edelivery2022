using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record SendNotificationCommandHandler(
        IUnitOfWork UnitOfWork,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<SendNotificationCommand, Unit>
    {
        public async Task<Unit> Handle(
            SendNotificationCommand command,
            CancellationToken ct)
        {
            string? email = command.LegalEntity?.Email ?? command.Individual?.Email;
            string? phone = command.LegalEntity?.Phone ?? command.Individual?.Phone;

            if (!string.IsNullOrEmpty(email) && command.Email != null)
            {
                await this.QueueMessagesService.PostMessageAsync(
                    new EmailQueueMessage(
                        QueueMessageFeatures.Notifications,
                        email,
                        command.Email.Subject,
                        command.Email.Body,
                        true,
                        new
                        {
                            command.ProfileId,
                            Source = "Esb notification endpoint",
                        }),
                    QueueMessageFeatures.Notifications,
                    ct);
            }

            if (!string.IsNullOrEmpty(phone))
            {
                if (!string.IsNullOrEmpty(command.Sms) && !string.IsNullOrEmpty(command.Viber))
                {
                    await this.QueueMessagesService.PostMessageAsync(
                        new ViberQueueMessage(
                            QueueMessageFeatures.Notifications,
                            phone,
                            command.Viber,
                            new
                            {
                                command.ProfileId,
                                Source = "Esb notification endpoint",
                                FallbackSmsBody = command.Sms,
                            }),
                        QueueMessageFeatures.Notifications,
                        ct);
                }
                else if (!string.IsNullOrEmpty(command.Sms))
                {
                    await this.QueueMessagesService.PostMessageAsync(
                        new SmsQueueMessage(
                            QueueMessageFeatures.Notifications,
                            phone,
                            command.Sms,
                            new
                            {
                                command.ProfileId,
                                Source = "Esb notification endpoint",
                            }),
                        QueueMessageFeatures.Notifications,
                        ct);
                }
                else if (!string.IsNullOrEmpty(command.Viber))
                {
                    await this.QueueMessagesService.PostMessageAsync(
                        new ViberQueueMessage(
                            QueueMessageFeatures.Notifications,
                            phone,
                            command.Viber,
                            new
                            {
                                command.ProfileId,
                                Source = "Esb notification endpoint",
                            }),
                        QueueMessageFeatures.Notifications,
                        ct);
                }
            }

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
