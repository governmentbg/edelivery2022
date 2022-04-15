using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<int> GetProfileTargetGroupAsync(
            int profileId,
            CancellationToken ct);
    }
}
