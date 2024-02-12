using MediatR;

namespace ED.Domain
{
    public record UpdateLoginProfileNotificationsByAdminCommand(
        int ProfileId,
        int LoginId,
        string Email,
        string Phone,
        bool EmailNotificationActive,
        bool EmailNotificationOnDeliveryActive,
        bool SmsNotificationActive,
        bool SmsNotificationOnDeliveryActive,
        bool ViberNotificationActive,
        bool ViberNotificationOnDeliveryActive,
        int AdminUserId,
        string Ip)
        : IRequest;
}
