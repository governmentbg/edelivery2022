namespace ED.Domain
{
    partial class ProfileServiceQueryRepository : Repository, IProfileServiceQueryRepository
    {
        public ProfileServiceQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
