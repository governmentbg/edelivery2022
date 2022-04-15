namespace ED.Domain
{
    partial class ProfileSeosQueryRepository : Repository, IProfileSeosQueryRepository
    {
        public ProfileSeosQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
