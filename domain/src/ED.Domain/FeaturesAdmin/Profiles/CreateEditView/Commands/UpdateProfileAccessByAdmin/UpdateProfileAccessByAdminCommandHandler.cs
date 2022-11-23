using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileAccessByAdminCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<UpdateProfileAccessByAdminCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateProfileAccessByAdminCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            IAdminProfilesCreateEditViewQueryRepository.GetLoginInfoVO loginInfo =
                await this.AdminProfilesCreateEditViewQueryRepository.GetLoginInfoAsync(
                    command.LoginId,
                    profile.ElectronicSubjectId,
                    ct);

            profile.UpdateLoginAccess(
                command.LoginId,
                command.Permissions
                    .Select(e => (e.Permission, e.TemplateId))
                    .ToArray());

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
                profile.Id,
                ProfileHistoryAction.GrantAccessToProfile,
                command.AdminUserId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.GrantAccessToProfile,
                    loginInfo.ElectronicSubjectId,
                    loginInfo.ElectronicSubjectName,
                    string.Empty),
                command.IP);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
