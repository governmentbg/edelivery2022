namespace ED.Domain
{
    partial class MessageListQueryRepository : Repository, IMessageListQueryRepository
    {
        public MessageListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
