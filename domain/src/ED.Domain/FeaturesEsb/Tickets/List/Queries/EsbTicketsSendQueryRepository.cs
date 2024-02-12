namespace ED.Domain
{
    partial class EsbTicketsListQueryRepository : Repository, IEsbTicketsListQueryRepository
    {
        public EsbTicketsListQueryRepository(
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
