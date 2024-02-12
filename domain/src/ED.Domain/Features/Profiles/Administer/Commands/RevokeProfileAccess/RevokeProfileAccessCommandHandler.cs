using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RevokeProfileAccessCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAggregateRepository<Login> LoginAggregateRepository)
        : IRequestHandler<RevokeProfileAccessCommand, Unit>
    {
        public async Task<Unit> Handle(
            RevokeProfileAccessCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            profile.RevokeAccess(command.LoginId);

            Login login = await this.LoginAggregateRepository.FindAsync(
                command.LoginId,
                ct);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByLogin(
               profile.Id,
               ProfileHistoryAction.RemoveAccessToProfile,
               command.ActionLoginId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.RemoveAccessToProfile,
                    login.ElectronicSubjectId,
                    login.ElectronicSubjectName,
                    string.Empty),
               command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
