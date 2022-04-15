using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTargetGroupsCreateEditViewQueryRepository
    {
        Task<GetTargetGroupVO> GetTargetGroupAsync(
            int targetGroupId,
            CancellationToken ct);
    }
}
