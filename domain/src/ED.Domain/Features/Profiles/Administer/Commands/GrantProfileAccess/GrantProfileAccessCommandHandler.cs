using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record GrantProfileAccessCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IProfileAdministerQueryRepository ProfileAdministerQueryRepository)
        : IRequestHandler<GrantProfileAccessCommand, Unit>
    {
        public async Task<Unit> Handle(
            GrantProfileAccessCommand command,
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

            string email = profile.EmailAddress;
            string phone = profile.Phone;

            if (!loginInfo.IsProfileLogin)
            {
                email = loginInfo.Email ?? email;
                phone = loginInfo.Phone ?? phone;
            }

            profile.GrantLoginAccess(
                command.LoginId,
                command.IsDefault,
                command.IsEmailNotificationEnabled,
                command.IsEmailNotificationOnDeliveryEnabled,
                command.IsSmsNotificationEnabled,
                command.IsSmsNotificationOnDeliveryEnabled,
                command.IsViberNotificationEnabled,
                command.IsViberNotificationOnDeliveryEnabled,
                email,
                phone,
                command.ActionLoginId,
                command.Permissions
                    .Select(e => (e.Permission, e.TemplateId))
                    .ToArray());

            ProfilesHistory profilesHistory = new(
                profile.Id,
                ProfileHistoryAction.GrantAccessToProfile,
                command.ActionLoginId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.GrantAccessToProfile,
                    loginInfo.ElectronicSubjectId,
                    loginInfo.ElectronicSubjectName,
                    command.Details),
                command.IP);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
