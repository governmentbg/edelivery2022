namespace ED.Domain
{
    public partial interface ICodeMessageSendQueryRepository
    {
        public record GetNotificationRecipientsVO(
            int ProfileId,
            string ProfileName,
            string Email,
            string Phone);
    }
}
