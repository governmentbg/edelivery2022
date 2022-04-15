using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateIndividualNamesCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAggregateRepository<Login> LoginAggregateRepository,
        IProfileAdministerQueryRepository ProfileAdministerQueryRepository)
        : IRequestHandler<UpdateIndividualNamesCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateIndividualNamesCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            StringBuilder updatesLog = new();
            profile.UpdateIndividualName(
                command.FirstName,
                command.MiddleName,
                command.LastName,
                command.ActionLoginId,
                updatesLog);

            IProfileAdministerQueryRepository.GetLoginVO loginVO =
                await this.ProfileAdministerQueryRepository.GetLoginAsync(
                    profile.ElectronicSubjectId,
                    ct);

            Login login = await this.LoginAggregateRepository.FindAsync(
                loginVO.LoginId,
                ct);

            login.UpdateNames(profile.ElectronicSubjectName);

            ProfilesHistory profilesHistory = new(
                profile.Id,
                ProfileHistoryAction.ProfileUpdated,
                command.ActionLoginId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.ProfileUpdated,
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    updatesLog.ToString()),
                command.IP);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
