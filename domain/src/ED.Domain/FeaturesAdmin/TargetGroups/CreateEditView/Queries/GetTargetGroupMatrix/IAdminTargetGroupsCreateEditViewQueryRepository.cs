using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTargetGroupsCreateEditViewQueryRepository
    {
        Task<GetTargetGroupMatrixVO[]> GetTargetGroupMatrixAsync(
            int targetGroupId,
            CancellationToken ct);
    }
}
