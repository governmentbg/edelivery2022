using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateOrUpdateIntegrationLoginCommandHandler(
        IUnitOfWork UnitOfWork,
        LoginAggregateRepository LoginAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<CreateOrUpdateIntegrationLoginCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateOrUpdateIntegrationLoginCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            bool existingLogin =
                await this.AdminProfilesCreateEditViewQueryRepository.GetExistingLoginAsync(
                    profile.ElectronicSubjectId,
                    ct);

            if (existingLogin)
            {
                Login login = (await this.LoginAggregateRepository.FindLoginByElectronicSubjectId(
                    profile.ElectronicSubjectId,
                    ct))!;

                login.UpdateIntegrationData(
                    command.CertificateThumbPrint,
                    command.PushNotificationsUrl,
                    command.CanSendOnBehalfOf);

                profile.UpdateSettings(
                    login.Id,
                    command.EmailNotificationActive,
                    command.EmailNotificationOnDeliveryActive,
                    command.SmsNotificationActive,
                    command.SmsNotificationOnDeliveryActive,
                    command.ViberNotificationActive,
                    command.ViberNotificationOnDeliveryActive,
                    command.Email,
                    command.Phone);

                await this.UnitOfWork.SaveAsync(ct);
            }
            else
            {
                await using ITransaction transaction =
                    await this.UnitOfWork.BeginTransactionAsync(ct);

                Login login = new(
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    profile.EmailAddress,
                    profile.Phone,
                    "Integration-" + profile.ElectronicSubjectId,
                    command.CertificateThumbPrint,
                    command.PushNotificationsUrl,
                    command.CanSendOnBehalfOf);

                await this.LoginAggregateRepository.AddAsync(login, ct);

                await this.UnitOfWork.SaveAsync(ct);

                profile.GrantLoginAccess(
                    login.Id,
                    true,
                    command.EmailNotificationActive,
                    command.EmailNotificationOnDeliveryActive,
                    command.SmsNotificationActive,
                    command.SmsNotificationOnDeliveryActive,
                    command.ViberNotificationActive,
                    command.ViberNotificationOnDeliveryActive,
                    profile.EmailAddress,
                    profile.Phone,
                    Login.SystemLoginId,
                    new[]
                    {
                        (LoginProfilePermissionType.FullMessageAccess, (int?)null)
                    });

                await this.UnitOfWork.SaveAsync(ct);

                await transaction.CommitAsync(ct);
            }

            return default;
        }
    }
}
