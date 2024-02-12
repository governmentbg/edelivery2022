using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        Task<TableResultVO<GetOutboxQO>> GetOutboxAsync(
            int profileId,
            int offset,
            int limit,
            string? subject,
            string? profile,
            DateTime? from,
            DateTime? to,
            string? urn,
            CancellationToken ct);
    }
}
