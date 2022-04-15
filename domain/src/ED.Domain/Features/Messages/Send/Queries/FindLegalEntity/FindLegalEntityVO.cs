namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        public record FindLegalEntityVO(
            int ProfileId,
            string Name);
    }
}
