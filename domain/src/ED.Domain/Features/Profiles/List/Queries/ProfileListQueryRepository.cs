namespace ED.Domain
{
    partial class ProfileListQueryRepository : Repository, IProfileListQueryRepository
    {
        public ProfileListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
