using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateLoginProfileNotificationsByAdminCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<UpdateLoginProfileNotificationsByAdminCommand, Unit>
    {
        private const string ActionDetails = "Change notification settings by admin";

        public async Task<Unit> Handle(
            UpdateLoginProfileNotificationsByAdminCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            profile.UpdateSettingsByAdmin(
                command.LoginId,
                command.EmailNotificationActive,
                command.EmailNotificationOnDeliveryActive,
                command.SmsNotificationActive,
                command.SmsNotificationOnDeliveryActive,
                command.ViberNotificationActive,
                command.ViberNotificationOnDeliveryActive,
                command.Email,
                command.Phone,
                command.AdminUserId);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
               profile.Id,
               ProfileHistoryAction.ProfileUpdated,
               command.AdminUserId,
               ProfilesHistory.GenerateAccessDetails(
                   ProfileHistoryAction.ProfileUpdated,
                   profile.ElectronicSubjectId,
                   profile.ElectronicSubjectName,
                   ActionDetails),
               command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
