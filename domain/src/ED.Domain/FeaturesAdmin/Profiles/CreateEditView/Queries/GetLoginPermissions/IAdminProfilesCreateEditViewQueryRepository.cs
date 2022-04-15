using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetLoginPermissionsVO> GetLoginPermissionsAsync(
            int profileId,
            int loginId,
            CancellationToken ct);
    }
}
