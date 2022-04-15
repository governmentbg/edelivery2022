namespace ED.Domain
{
    partial class BlobListQueryRepository : Repository, IBlobListQueryRepository
    {
        public BlobListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
