namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record FindRecipientLegalEntityVO(
            int ProfileId,
            string Name);
    }
}
