namespace ED.Domain
{
    partial class TicketsListQueryRepository : Repository, ITicketsListQueryRepository
    {
        public TicketsListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
