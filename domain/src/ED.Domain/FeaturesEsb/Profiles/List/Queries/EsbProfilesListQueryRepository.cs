namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : Repository, IEsbProfilesListQueryRepository
    {
        public EsbProfilesListQueryRepository(
            UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
