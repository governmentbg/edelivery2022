using Microsoft.Extensions.Options;

namespace ED.Domain
{
    partial class EsbMessagesListQueryRepository : Repository, IEsbMessagesListQueryRepository
    {
        private readonly DomainOptions domainOptions;

        public EsbMessagesListQueryRepository(
            UnitOfWork unitOfWork,
            IOptions<DomainOptions> DomainOptionsAccessor)
            : base(unitOfWork)
        {
            this.domainOptions = DomainOptionsAccessor.Value;
        }
    }
}
