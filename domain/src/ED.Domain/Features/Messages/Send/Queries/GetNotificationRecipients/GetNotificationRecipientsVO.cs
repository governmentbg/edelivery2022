namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record GetNotificationRecipientsVO(
            int ProfileId,
            string ProfileName,
            int LoginId,
            string LoginName,
            bool IsEmailNotificationEnabled,
            string Email,
            bool IsSmsNotificationEnabled,
            bool IsViberNotificationEnabled,
            string Phone,
            string? PushNotificationUrl);
    }
}
