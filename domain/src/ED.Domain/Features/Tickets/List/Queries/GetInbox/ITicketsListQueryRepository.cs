using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITicketsListQueryRepository
    {
        Task<TableResultVO<GetInboxQO>> GetInboxAsync(
            int profileId,
            int offset,
            int limit,
            DateTime? from,
            DateTime? to,
            CancellationToken ct);
    }
}
