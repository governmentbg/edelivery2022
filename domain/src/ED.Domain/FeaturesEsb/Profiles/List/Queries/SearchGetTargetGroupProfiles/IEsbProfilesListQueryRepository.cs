using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<SearchGetTargetGroupProfilesVO?> SearchGetTargetGroupProfilesAsync(
            string identifier,
            int? templateId,
            int targetGroupId,
            CancellationToken ct);
    }
}
