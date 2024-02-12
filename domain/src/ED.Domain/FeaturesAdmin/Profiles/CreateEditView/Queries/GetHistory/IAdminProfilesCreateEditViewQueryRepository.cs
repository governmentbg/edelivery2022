using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<TableResultVO<GetHistoryVO>> GetHistoryAsync(
            int profileId,
            ProfileHistoryAction[] actions,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
