namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        public record GetRecipientOrSenderProfileVO(
            int ProfileId,
            string ProfileName);
    }
}
