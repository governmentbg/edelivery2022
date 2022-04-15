namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetMessageSenderVO(
            int ProfileId,
            string ProfileName);
    }
}
