namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetIntegrationLoginInfoVO(
            string? CertificateThumbPrint,
            string? PushNotificationsUrl,
            bool? CanSendOnBehalfOf,
            bool PhoneNotificationActive,
            bool PhoneNotificationOnDeliveryActive,
            bool EmailNotificationActive,
            bool EmailNotificationOnDeliveryActive,
            string Email,
            string Phone);
    }
}
