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
        bool PhoneNotificationActive,
        bool PhoneNotificationOnDeliveryActive,
        int AdminUserId,
        string Ip)
        : IRequest;
}
