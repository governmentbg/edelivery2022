using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetProfileBasicInfoVO> GetProfileBasicInfoAsync(
            int profileId,
            CancellationToken ct);
    }
}
