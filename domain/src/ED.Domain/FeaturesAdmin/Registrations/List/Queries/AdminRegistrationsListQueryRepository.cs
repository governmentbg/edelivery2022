using Microsoft.Extensions.Logging;

namespace ED.Domain
{
    partial class AdminRegistrationsListQueryRepository : Repository, IAdminRegistrationsListQueryRepository
    {
        private ILogger logger;

        public AdminRegistrationsListQueryRepository(
            UnitOfWork unitOfWork,
            ILogger<AdminProfilesListQueryRepository> logger)
            : base(unitOfWork)
        {
            this.logger = logger;
        }
    }
}
