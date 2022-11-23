namespace ED.Domain
{
    partial class EsbBlobsListQueryRepository : Repository, IEsbBlobsListQueryRepository
    {
        public EsbBlobsListQueryRepository(
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
