using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesListQueryRepository
    {
        Task<TableResultVO<GetInboxVO>> GetInboxAsync(
            int profileId,
            DateTime? from,
            DateTime? to,
            int? templateId,
            int? offset,
            int? limit,
            CancellationToken ct);
    }
}
