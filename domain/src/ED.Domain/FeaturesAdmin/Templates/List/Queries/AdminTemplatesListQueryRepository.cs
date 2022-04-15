namespace ED.Domain
{
    partial class AdminTemplatesListQueryRepository : Repository, IAdminTemplatesListQueryRepository
    {
        public AdminTemplatesListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
