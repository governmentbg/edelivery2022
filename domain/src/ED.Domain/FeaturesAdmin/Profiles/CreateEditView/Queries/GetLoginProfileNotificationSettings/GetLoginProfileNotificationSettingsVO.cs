namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        public record GetLoginProfileNotificationSettingsVO(
            string Email,
            string Phone,
            bool EmailNotificationActive,
            bool EmailNotificationOnDeliveryActive,
            bool SmsNotificationActive,
            bool SmsNotificationOnDeliveryActive,
            bool ViberNotificationActive,
            bool ViberNotificationOnDeliveryActive);
    }
}
