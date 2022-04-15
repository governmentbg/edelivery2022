namespace ED.Domain
{
    partial class IntegrationServiceMessagesListQueryRepository : Repository, IIntegrationServiceMessagesListQueryRepository
    {
        public IntegrationServiceMessagesListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
