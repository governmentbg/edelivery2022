using MediatR;

namespace ED.Domain
{
    public record CreateOrUpdateIntegrationLoginCommand(
        int ProfileId,
        string CertificateThumbPrint,
        string PushNotificationsUrl,
        bool CanSendOnBehalfOf,
        bool SmsNotificationActive,
        bool SmsNotificationOnDeliveryActive,
        bool EmailNotificationActive,
        bool EmailNotificationOnDeliveryActive,
        bool ViberNotificationActive,
        bool ViberNotificationOnDeliveryActive,
        string Email,
        string Phone)
        : IRequest;
}
