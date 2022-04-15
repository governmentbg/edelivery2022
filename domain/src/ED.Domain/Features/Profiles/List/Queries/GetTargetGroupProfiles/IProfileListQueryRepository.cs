using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        Task<TableResultVO<GetTargetGroupProfilesVO>> GetTargetGroupProfilesAsync(
            int targetGroupId,
            string term,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
