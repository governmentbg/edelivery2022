using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileSettingsCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository)
        : IRequestHandler<UpdateProfileSettingsCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateProfileSettingsCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            profile.UpdateSettings(
                command.LoginId,
                command.IsEmailNotificationEnabled,
                command.IsEmailNotificationOnDeliveryEnabled,
                command.IsSmsNotificationEnabled,
                command.IsSmsNotificationOnDeliveryEnabled,
                command.IsViberNotificationEnabled,
                command.IsViberNotificationOnDeliveryEnabled,
                command.Email,
                command.Phone);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
