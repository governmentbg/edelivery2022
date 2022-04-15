using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<TableResultVO<GetTargetGroupsVO>> GetTargetGroupsAsync(
            int profileId,
            CancellationToken ct);
    }
}
