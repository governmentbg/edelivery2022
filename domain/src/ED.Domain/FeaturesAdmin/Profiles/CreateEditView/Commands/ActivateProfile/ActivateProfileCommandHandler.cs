using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ActivateProfileCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<ActivateProfileCommand, ActiveProfileCommandResult>
    {
        public async Task<ActiveProfileCommandResult> Handle(
            ActivateProfileCommand command,
            CancellationToken ct)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileBasicInfoVO info =
                await this.AdminProfilesCreateEditViewQueryRepository
                    .GetProfileBasicInfoAsync(
                        command.ProfileId,
                        ct);

            bool hasActiveOrPendingProfile =
                await this.AdminProfilesCreateEditViewQueryRepository
                    .HasActiveOrPendingProfileAsync(
                        command.ProfileId,
                        info.Identifier,
                        info.TargetGroupId,
                        ct);

            if (hasActiveOrPendingProfile)
            {
                return new ActiveProfileCommandResult(
                    false,
                    "There is an active profile or pending registration with the same identifier.");
            }

            Profile profile =
                await this.ProfileAggregateRepository
                    .FindAsync(command.ProfileId, ct);

            profile.Activate(command.AdminUserId);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
                command.ProfileId,
                ProfileHistoryAction.ProfileActivated,
                command.AdminUserId,
                null,
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return new ActiveProfileCommandResult(true, string.Empty);
        }
    }
}
