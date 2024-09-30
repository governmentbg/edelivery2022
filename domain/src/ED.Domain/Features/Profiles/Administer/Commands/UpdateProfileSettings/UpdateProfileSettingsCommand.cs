using MediatR;

namespace ED.Domain
{
    public record UpdateProfileSettingsCommand(
        int ProfileId,
        int LoginId,
        bool IsEmailNotificationEnabled,
        bool IsEmailNotificationOnDeliveryEnabled,
        bool IsPhoneNotificationEnabled,
        bool IsPhoneNotificationOnDeliveryEnabled,
        string Email,
        string Phone)
        : IRequest;
}
