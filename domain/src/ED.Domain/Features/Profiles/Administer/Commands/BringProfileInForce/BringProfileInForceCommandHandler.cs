using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record BringProfileInForceCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAggregateRepository<Login> LoginAggregateRepository,
        IProfileAdministerQueryRepository ProfileAdministerQueryRepository)
        : IRequestHandler<BringProfileInForceCommand, Unit>
    {
        public async Task<Unit> Handle(
            BringProfileInForceCommand command,
            CancellationToken ct)
        {
            int profileId = await this.ProfileAdministerQueryRepository
                .GetProfileByLoginIdAsync(
                    command.LoginId,
                    ct);

            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                profileId,
                ct);

            profile.BringInForce();
            profile.GrantLoginAccess(
                command.LoginId,
                true,
                command.IsEmailNotificationEnabled,
                command.IsEmailNotificationOnDeliveryEnabled,
                command.IsSmsNotificationEnabled,
                command.IsSmsNotificationOnDeliveryEnabled,
                command.IsViberNotificationEnabled,
                command.IsViberNotificationOnDeliveryEnabled,
                profile.EmailAddress,
                profile.Phone,
                command.LoginId,
                new[]
                {
                    (LoginProfilePermissionType.OwnerAccess, (int?)null)
                });

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByLogin(
                profile.Id,
                ProfileHistoryAction.BringProfileInForce,
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
