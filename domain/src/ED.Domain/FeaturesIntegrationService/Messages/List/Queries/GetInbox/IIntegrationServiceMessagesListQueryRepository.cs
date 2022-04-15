using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesListQueryRepository
    {
        Task<TableResultVO<GetInboxVO>> GetInboxAsync(
            string certificateThumbprint,
            bool showNewOnly,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
