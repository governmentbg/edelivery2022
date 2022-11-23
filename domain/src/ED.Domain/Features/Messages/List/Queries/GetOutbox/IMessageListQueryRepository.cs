using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            int profileId,
            int offset,
            int limit,
            string? subject,
            string? profile,
            DateTime? fromDate,
            DateTime? toDate,
            string? urn,
            CancellationToken ct);
    }
}
