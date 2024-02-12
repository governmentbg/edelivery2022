namespace ED.Domain
{
    partial class TicketsOpenQueryRepository : Repository, ITicketsOpenQueryRepository
    {
        public TicketsOpenQueryRepository(
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
