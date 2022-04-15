using Microsoft.Extensions.Options;

namespace ED.Domain
{
    partial class EsbMessagesSendQueryRepository : Repository, IEsbMessagesSendQueryRepository
    {
        private readonly DomainOptions domainOptions;

        public EsbMessagesSendQueryRepository(
            UnitOfWork unitOfWork,
            IOptions<DomainOptions> DomainOptionsAccessor)
            : base(unitOfWork)
        {
            this.domainOptions = DomainOptionsAccessor.Value;
        }
    }
}
