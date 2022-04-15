namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record FindIndividualVO(
            int ProfileId,
            string Name);
    }
}
