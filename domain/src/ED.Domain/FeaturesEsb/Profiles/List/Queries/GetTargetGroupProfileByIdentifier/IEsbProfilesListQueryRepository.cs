using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<GetTargetGroupProfileByIdentifierVO?> GetTargetGroupProfileByIdentifierAsync(
            int targetGroupId,
            string identifier,
            CancellationToken ct);
    }
}
