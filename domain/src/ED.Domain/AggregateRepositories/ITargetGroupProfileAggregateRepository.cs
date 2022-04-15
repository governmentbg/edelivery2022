using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public interface ITargetGroupProfileAggregateRepository : IAggregateRepository<TargetGroupProfile>
    {
        Task<TargetGroupProfile> FindAsync(int targetGroupId, int profileId, CancellationToken ct);
    }
}
