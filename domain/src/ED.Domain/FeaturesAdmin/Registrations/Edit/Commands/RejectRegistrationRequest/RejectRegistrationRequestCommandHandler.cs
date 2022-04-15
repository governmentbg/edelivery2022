using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RejectRegistrationRequestCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RegistrationRequest> RegistrationRequestAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminRegistrationsEditQueryRepository AdminRegistrationsEditQueryRepository,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<RejectRegistrationRequestCommand>
    {
        private const string NotificationEvent = "OnRegistrationRejected";

        public async Task<Unit> Handle(
            RejectRegistrationRequestCommand command,
            CancellationToken ct)
        {
            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            RegistrationRequest registrationRequest =
                await this.RegistrationRequestAggregateRepository.FindAsync(
                    command.RegistrationRequestId,
                    ct);

            registrationRequest.Reject(command.AdminUserId, command.Comment);

            await this.UnitOfWork.SaveAsync(ct);

            ProfilesHistory profilesHistory = new(
                registrationRequest.RegisteredProfileId,
                ProfileHistoryAction.ProfileDeactivated,
                command.AdminUserId);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            EmailQueueMessage emailQueueMessage = await this.GetNotification(
                registrationRequest.RegistrationRequestId,
                registrationRequest.RegistrationEmail,
                registrationRequest.CreatedBy,
                registrationRequest.RegisteredProfileId,
                ct);

            await this.QueueMessagesService.PostMessageAsync(
                emailQueueMessage,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return default;
        }

        private async Task<EmailQueueMessage> GetNotification(
            int registrationRequestId,
            string registrationRequestRecipient,
            int registrationRequestCreatedBy,
            int registrationRequestRegisteredProfileId,
            CancellationToken ct)
        {
            string registrationRequestAuthor =
               await this.AdminRegistrationsEditQueryRepository.GetLoginNameAsync(
                   registrationRequestCreatedBy,
                   ct);

            string registrationRequestProfile =
                await this.AdminRegistrationsEditQueryRepository.GetProfileNameAsync(
                    registrationRequestRegisteredProfileId,
                    ct);

            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.RegistrationRequestRejectedEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.RegistrationRequestRejectedEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.RegistrationRequestRejectedEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.RegistrationRequestRejectedEmailBody)}");

            return new EmailQueueMessage(
                registrationRequestRecipient,
                string.Format(emailSubect, registrationRequestProfile),
                string.Format(
                    emailBody,
                    registrationRequestAuthor,
                    registrationRequestProfile),
                false,
                new
                {
                    Event = NotificationEvent,
                    RegistrationRequestId = registrationRequestId
                });
        }
    }
}
