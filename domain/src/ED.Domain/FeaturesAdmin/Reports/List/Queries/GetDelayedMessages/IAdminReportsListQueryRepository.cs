using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        Task<TableResultVO<GetDelayedMessagesVO>> GetDelayedMessagesAsync(
            int adminUserId,
            int delay,
            int targetGroupId,
            int? profileId,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
