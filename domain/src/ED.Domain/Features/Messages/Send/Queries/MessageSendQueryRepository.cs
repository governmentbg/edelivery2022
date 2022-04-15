namespace ED.Domain
{
    partial class MessageSendQueryRepository : Repository, IMessageSendQueryRepository
    {
        public MessageSendQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
