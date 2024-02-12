using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileAccessCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IProfileAdministerQueryRepository ProfileAdministerQueryRepository)
        : IRequestHandler<UpdateProfileAccessCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateProfileAccessCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            IProfileAdministerQueryRepository.GetLoginInfoVO loginInfo =
                await this.ProfileAdministerQueryRepository.GetLoginInfoAsync(
                    command.LoginId,
                    profile.ElectronicSubjectId,
                    ct);

            profile.UpdateLoginAccess(
                command.LoginId,
                command.Permissions
                    .Select(e => (e.Permission, e.TemplateId))
                    .ToArray());

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByLogin(
                profile.Id,
                ProfileHistoryAction.GrantAccessToProfile,
                command.ActionLoginId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.GrantAccessToProfile,
                    loginInfo.ElectronicSubjectId,
                    loginInfo.ElectronicSubjectName,
                    command.Details),
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
