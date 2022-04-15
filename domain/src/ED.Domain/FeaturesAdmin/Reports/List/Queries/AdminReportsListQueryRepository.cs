using Microsoft.Extensions.Logging;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : Repository, IAdminReportsListQueryRepository
    {
        private ILogger logger;

        public AdminReportsListQueryRepository(
            UnitOfWork unitOfWork,
            ILogger<AdminReportsListQueryRepository> logger)
            : base(unitOfWork)
        {
            this.logger = logger;
        }
    }
}
