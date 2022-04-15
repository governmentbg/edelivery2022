using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetIntegrationLoginInfoVO?> GetIntegrationLoginInfoAsync(
            int profileId,
            CancellationToken ct);
    }
}
