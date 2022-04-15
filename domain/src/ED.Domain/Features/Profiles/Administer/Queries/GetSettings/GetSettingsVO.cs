namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetSettingsVO(
            bool IsEmailNotificationEnabled,
            bool IsEmailNotificationOnDeliveryEnabled,
            bool IsSmsNotificationEnabled,
            bool IsSmsNotificationOnDeliveryEnabled,
            bool IsViberNotificationEnabled,
            bool IsViberNotificationOnDeliveryEnabled,
            string Email,
            string Phone);
    }
}
