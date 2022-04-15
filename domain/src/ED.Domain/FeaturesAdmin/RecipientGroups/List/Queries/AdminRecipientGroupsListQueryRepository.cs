namespace ED.Domain
{
    partial class AdminRecipientGroupsListQueryRepository : Repository, IAdminRecipientGroupsListQueryRepository
    {
        public AdminRecipientGroupsListQueryRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
