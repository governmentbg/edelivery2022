using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<TableResultVO<GetTargetGroupProfilesVO>> GetTargetGroupProfilesAsync(
            int targetGroupId,
            int? offset,
            int? limit,
            CancellationToken ct);
    }
}
