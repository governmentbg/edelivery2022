namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetNotificationRecipientsOnDeliveryVO(
            int ProfileId,
            string ProfileName,
            int LoginId,
            string LoginName,
            bool IsEmailNotificationOnDeliveryEnabled,
            string Email,
            bool IsSmsNotificationOnDeliveryEnabled,
            bool IsViberNotificationOnDeliveryEnabled,
            string Phone,
            string? PushNotificationUrl);
    }
}
