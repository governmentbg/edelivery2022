namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetSettingsVO(
            bool IsEmailNotificationEnabled,
            bool IsEmailNotificationOnDeliveryEnabled,
            bool IsPhoneNotificationEnabled,
            bool IsPhoneNotificationOnDeliveryEnabled,
            string Email,
            string Phone);
    }
}
