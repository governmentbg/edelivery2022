namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetIntegrationLoginInfoVO(
            string? CertificateThumbPrint,
            string? PushNotificationsUrl,
            bool? CanSendOnBehalfOf,
            bool SmsNotificationActive,
            bool SmsNotificationOnDeliveryActive,
            bool EmailNotificationActive,
            bool EmailNotificationOnDeliveryActive,
            bool ViberNotificationActive,
            bool ViberNotificationOnDeliveryActive,
            string Email,
            string Phone);
    }
}
