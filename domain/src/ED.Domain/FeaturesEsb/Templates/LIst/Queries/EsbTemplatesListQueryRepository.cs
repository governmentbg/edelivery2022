namespace ED.Domain
{
    partial class EsbTemplatesListQueryRepository : Repository, IEsbTemplatesListQueryRepository
    {
        public EsbTemplatesListQueryRepository(
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
