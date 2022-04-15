namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        public record GetNotificationRecipientsVO(
            int ProfileId,
            string ProfileName,
            string Email,
            string Phone);
    }
}
