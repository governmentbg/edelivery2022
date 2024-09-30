namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetNotificationRecipientsVO(
            int ProfileId,
            string ProfileName,
            int LoginId,
            string LoginName,
            bool IsEmailNotificationEnabled,
            string Email,
            bool IsPhoneNotificationEnabled,
            string Phone,
            string? PushNotificationUrl);
    }
}
