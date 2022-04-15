using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTargetGroupsListQueryRepository
    {
        Task<GetTargetGroupsVO[]> GetTargetGroupsAsync(
            int profileId,
            CancellationToken ct);
    }
}
