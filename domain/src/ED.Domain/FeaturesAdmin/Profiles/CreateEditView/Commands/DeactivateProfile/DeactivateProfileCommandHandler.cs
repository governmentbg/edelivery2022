using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeactivateProfileCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<DeactivateProfileCommand, DeactivateProfileCommandResult>
    {
        public async Task<DeactivateProfileCommandResult> Handle(
            DeactivateProfileCommand command,
            CancellationToken ct)
        {
            Profile profile =
                await this.ProfileAggregateRepository
                    .FindAsync(command.ProfileId, ct);

            profile.Deactivate(command.AdminUserId);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
                command.ProfileId,
                ProfileHistoryAction.ProfileDeactivated,
                command.AdminUserId,
                null,
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return new DeactivateProfileCommandResult(true, string.Empty);
        }
    }
}
