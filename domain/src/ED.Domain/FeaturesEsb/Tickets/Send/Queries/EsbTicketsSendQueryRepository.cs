using Microsoft.Extensions.Options;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : Repository, IEsbTicketsSendQueryRepository
    {
        private readonly DomainOptions domainOptions;

        public EsbTicketsSendQueryRepository(
            UnitOfWork unitOfWork,
            IOptions<DomainOptions> DomainOptionsAccessor)
            : base(unitOfWork)
        {
            this.domainOptions = DomainOptionsAccessor.Value;
        }
    }
}
