namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        public record GetNotificationRecipientsOnDeliveryVO(
            int ProfileId,
            string ProfileName,
            int LoginId,
            string LoginName,
            bool IsEmailNotificationOnDeliveryEnabled,
            string Email,
            bool IsPhoneNotificationOnDeliveryEnabled,
            string Phone,
            string? PushNotificationUrl);
    }
}
