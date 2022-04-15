using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetProfileQuotasInfoVO> GetProfileQuotasInfoAsync(
            int profileId,
            CancellationToken ct);
    }
}
