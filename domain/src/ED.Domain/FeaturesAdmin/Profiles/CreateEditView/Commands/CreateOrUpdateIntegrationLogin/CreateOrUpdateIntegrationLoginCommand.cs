using MediatR;

namespace ED.Domain
{
    public record CreateOrUpdateIntegrationLoginCommand(
        int ProfileId,
        string CertificateThumbPrint,
        string PushNotificationsUrl,
        bool CanSendOnBehalfOf,
        bool PhoneNotificationActive,
        bool PhoneNotificationOnDeliveryActive,
        bool EmailNotificationActive,
        bool EmailNotificationOnDeliveryActive,
        string Email,
        string Phone)
        : IRequest;
}
