using MediatR;

namespace ED.Domain
{
    public record UpdateProfileSettingsCommand(
        int ProfileId,
        int LoginId,
        bool IsEmailNotificationEnabled,
        bool IsEmailNotificationOnDeliveryEnabled,
        bool IsSmsNotificationEnabled,
        bool IsSmsNotificationOnDeliveryEnabled,
        bool IsViberNotificationEnabled,
        bool IsViberNotificationOnDeliveryEnabled,
        string Email,
        string Phone)
        : IRequest;
}
