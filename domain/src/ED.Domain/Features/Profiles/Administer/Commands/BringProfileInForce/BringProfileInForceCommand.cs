using MediatR;

namespace ED.Domain
{
    public record BringProfileInForceCommand(
        int LoginId,
        bool IsEmailNotificationEnabled,
        bool IsEmailNotificationOnDeliveryEnabled,
        bool IsPhoneNotificationEnabled,
        bool IsPhoneNotificationOnDeliveryEnabled,
        string Ip)
        : IRequest;
}
