using Microsoft.Extensions.Options;

namespace ED.Domain
{
    partial class JobsDataPortalListQueryRepository : Repository, IJobsDataPortalListQueryRepository
    {
        private readonly DomainOptions domainOptions;

        public JobsDataPortalListQueryRepository(
            UnitOfWork unitOfWork,
            IOptions<DomainOptions> DomainOptionsAccessor)
            : base(unitOfWork)
        {
            this.domainOptions = DomainOptionsAccessor.Value;
        }
    }
}
