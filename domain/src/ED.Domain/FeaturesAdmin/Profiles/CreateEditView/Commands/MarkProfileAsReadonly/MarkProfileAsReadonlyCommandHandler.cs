using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record MarkProfileAsReadonlyCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<MarkProfileAsReadonlyCommand, Unit>
    {
        public async Task<Unit> Handle(
            MarkProfileAsReadonlyCommand command,
            CancellationToken ct)
        {
            Profile profile =
                await this.ProfileAggregateRepository
                    .FindAsync(command.ProfileId, ct);

            profile.MarkAsReadOnly(command.AdminUserId);

            ProfilesHistory profilesHistory = new(
                command.ProfileId,
                ProfileHistoryAction.MarkAsReadonly,
                command.AdminUserId)
            {
                IPAddress = command.Ip
            };

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
