using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record MarkProfileAsNonReadonlyCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<MarkProfileAsNonReadonlyCommand, Unit>
    {
        public async Task<Unit> Handle(
            MarkProfileAsNonReadonlyCommand command,
            CancellationToken ct)
        {
            Profile profile =
                await this.ProfileAggregateRepository
                    .FindAsync(command.ProfileId, ct);

            profile.MarkAsNonReadonly(command.AdminUserId);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
                command.ProfileId,
                ProfileHistoryAction.MarkAsNonReadonly,
                command.AdminUserId,
                null,
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
