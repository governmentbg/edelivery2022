using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<bool> HasActiveOrPendingProfileAsync(
            int profileId,
            string identifier,
            int targetGroupId,
            CancellationToken ct);
    }
}
