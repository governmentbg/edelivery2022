using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesListQueryRepository
    {
        Task<TableResultVO<GetOutboxVO>> GetOutboxAsync(
            string certificateThumbprint,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
