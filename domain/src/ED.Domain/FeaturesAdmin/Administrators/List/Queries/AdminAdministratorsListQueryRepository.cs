namespace ED.Domain
{
    partial class AdminAdministratorsListQueryRepository : Repository, IAdminAdministratorsListQueryRepository
    {
        public AdminAdministratorsListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
