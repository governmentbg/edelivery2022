using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        Task<GetForwardHistoryVO[]> GetForwardHistoryAsync(
            int messageId,
            CancellationToken ct);
    }
}
