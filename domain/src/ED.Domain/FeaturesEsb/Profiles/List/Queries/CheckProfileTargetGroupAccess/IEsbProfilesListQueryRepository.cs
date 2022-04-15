using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<bool> CheckProfileTargetGroupAccessAsync(
            int profileId,
            int targetGroupId,
            CancellationToken ct);
    }
}
