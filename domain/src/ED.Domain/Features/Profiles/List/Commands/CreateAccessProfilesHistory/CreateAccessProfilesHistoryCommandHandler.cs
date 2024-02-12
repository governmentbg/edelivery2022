using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateAccessProfilesHistoryCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository)
        : IRequestHandler<CreateAccessProfilesHistoryCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateAccessProfilesHistoryCommand command,
            CancellationToken ct)
        {
            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByLogin(
                command.ProfileId,
                ProfileHistoryAction.AccessProfile,
                command.LoginId,
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
