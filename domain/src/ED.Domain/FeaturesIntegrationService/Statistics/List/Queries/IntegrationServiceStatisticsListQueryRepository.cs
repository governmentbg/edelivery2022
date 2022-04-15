using Microsoft.Extensions.Logging;

namespace ED.Domain
{
    partial class IntegrationServiceStatisticsListQueryRepository : Repository, IIntegrationServiceStatisticsListQueryRepository
    {
        private ILogger logger;

        public IntegrationServiceStatisticsListQueryRepository(
            UnitOfWork unitOfWork,
            ILogger<IntegrationServiceStatisticsListQueryRepository> logger)
            : base(unitOfWork)
        {
            this.logger = logger;
        }
    }
}
