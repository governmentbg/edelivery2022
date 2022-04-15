using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record GrantProfileAccessByAdminCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<GrantProfileAccessByAdminCommand, Unit>
    {
        public async Task<Unit> Handle(
            GrantProfileAccessByAdminCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            int targetGroupId =
                await this.AdminProfilesCreateEditViewQueryRepository
                    .GetProfileTargetGroupAsync(command.ProfileId, ct);

            bool isCurrentProfilePublicAdministration =
                targetGroupId == TargetGroup.PublicAdministrationTargetGroupId;

            IAdminProfilesCreateEditViewQueryRepository.GetLoginInfoVO loginInfo =
               await this.AdminProfilesCreateEditViewQueryRepository.GetLoginInfoAsync(
                   command.LoginId,
                   profile.ElectronicSubjectId,
                   ct);

            string email = profile.EmailAddress;
            string phone = profile.Phone;

            if (!loginInfo.IsProfileLogin)
            {
                email = loginInfo.Email ?? email;
                phone = loginInfo.Phone ?? phone;
            }

            profile.GrantLoginAccessByAdmin(
                command.LoginId,
                false,
                true,
                false,
                !isCurrentProfilePublicAdministration,
                false,
                !isCurrentProfilePublicAdministration,
                false,
                email,
                phone,
                command.AdminUserId,
                command.Permissions
                    .Select(e => (e.Permission, e.TemplateId))
                    .ToArray());

            ProfilesHistory profilesHistory = new(
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
