using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetProfileEsbUserInfoVO> GetProfileEsbUserInfoAsync(
            int profileId,
            CancellationToken ct);
    }
}
