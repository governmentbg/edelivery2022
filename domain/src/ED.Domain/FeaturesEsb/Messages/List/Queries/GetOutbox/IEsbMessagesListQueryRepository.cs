using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesListQueryRepository
    {
        Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            int profileId,
            int? offset,
            int? limit,
            CancellationToken ct);
    }
}
