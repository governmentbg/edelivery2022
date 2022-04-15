namespace ED.Domain
{
    public partial interface IIntegrationServiceCodeMessagesSendQueryRepository
    {
        public record GetCodeSenderVO(
            int ProfileId,
            int LoginId);
    }
}
