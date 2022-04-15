namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        public record GetNewMessagesCountVO(
            int ProfileId,
            int Count);
    }
}
