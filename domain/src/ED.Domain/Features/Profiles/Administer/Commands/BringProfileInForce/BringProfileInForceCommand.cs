using MediatR;

namespace ED.Domain
{
    public record BringProfileInForceCommand(
        int LoginId,
        bool IsEmailNotificationEnabled,
        bool IsEmailNotificationOnDeliveryEnabled,
        bool IsSmsNotificationEnabled,
        bool IsSmsNotificationOnDeliveryEnabled,
        bool IsViberNotificationEnabled,
        bool IsViberNotificationOnDeliveryEnabled,
        string Ip)
        : IRequest;
}
