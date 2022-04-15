using Microsoft.Extensions.Logging;

namespace ED.Domain
{
    partial class AdminProfilesListQueryRepository : Repository, IAdminProfileListQueryRepository
    {
        private ILogger logger;

        public AdminProfilesListQueryRepository(
            UnitOfWork unitOfWork,
            ILogger<AdminProfilesListQueryRepository> logger)
            : base(unitOfWork)
        {
            this.logger = logger;
        }
    }
}
