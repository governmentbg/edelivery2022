using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        Task<TableResultVO<GetSentMessagesVO>> GetSentMessagesAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            int? recipientProfileId,
            int? senderProfileId,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
