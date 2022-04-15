using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetLoginProfileNotificationSettingsVO> GetLoginProfileNotificationSettingsAsync(
            int loginId,
            int profileId,
            CancellationToken ct);
    }
}
