namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record FindRecipientIndividualVO(
            int ProfileId,
            string Name);
    }
}
