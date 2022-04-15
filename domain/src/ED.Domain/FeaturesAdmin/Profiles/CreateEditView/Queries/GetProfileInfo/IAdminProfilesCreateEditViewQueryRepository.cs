using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetProfileInfoVO> GetProfileInfoAsync(
            int adminProfileId,
            int profileId,
            CancellationToken ct);
    }
}
