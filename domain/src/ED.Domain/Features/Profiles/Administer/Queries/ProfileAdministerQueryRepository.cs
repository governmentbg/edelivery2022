namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : Repository, IProfileAdministerQueryRepository
    {
        public ProfileAdministerQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
