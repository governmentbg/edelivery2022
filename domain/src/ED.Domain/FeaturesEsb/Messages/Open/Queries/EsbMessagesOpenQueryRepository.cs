using Microsoft.Extensions.Options;

namespace ED.Domain
{
    partial class EsbMessagesOpenQueryRepository : Repository, IEsbMessagesOpenQueryRepository
    {
        private readonly DomainOptions domainOptions;

        public EsbMessagesOpenQueryRepository(
            UnitOfWork unitOfWork,
            IOptions<DomainOptions> DomainOptionsAccessor)
            : base(unitOfWork)
        {
            this.domainOptions = DomainOptionsAccessor.Value;
        }
    }
}
