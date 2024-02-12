using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    internal class TargetGroupProfileAggregateRepository
        : AggregateRepository<TargetGroupProfile>, ITargetGroupProfileAggregateRepository
    {
        public TargetGroupProfileAggregateRepository(UnitOfWork unitOfWork)
            : base(unitOfWork)
        { }

        public Task<TargetGroupProfile> FindAsync(
            int targetGroupId,
            int profileId,
            CancellationToken ct)
        {
            return this.FindEntityAsync(
                new object[] { targetGroupId, profileId },
                ct);
        }
    }
}
