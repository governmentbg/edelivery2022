using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAggregateRepository<Login> LoginAggregateRepository,
        IProfileAdministerQueryRepository ProfileAdministerQueryRepository)
        : IRequestHandler<UpdateProfileCommand, UpdateProfileCommandResult>
    {
        public async Task<UpdateProfileCommandResult> Handle(
            UpdateProfileCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            bool isEmailUnique =
                await this.ProfileAdministerQueryRepository.CheckEmailUniquenessAsync(
                    profile.Identifier,
                    command.Email,
                    ct);

            if (!isEmailUnique)
            {
                return new(false, UpdateProfileCommandResultEnum.DuplicateEmail);
            }

            StringBuilder updatesLog = new();
            profile.Update(
                command.Email,
                command.Phone,
                command.Residence,
                command.Sync,
                command.ActionLoginId,
                updatesLog);

            IProfileAdministerQueryRepository.GetLoginVO loginVO =
                await this.ProfileAdministerQueryRepository.GetLoginAsync(
                    profile.ElectronicSubjectId,
                    ct);

            Login login = await this.LoginAggregateRepository.FindAsync(
                loginVO.LoginId,
                ct);

            login.Update(command.Email, command.Phone);

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

            return new(true, UpdateProfileCommandResultEnum.Ok);
        }
    }
}
