using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<GetForwardMessageInfoVO> GetForwardMessageInfoAsync(
            int messageId,
            CancellationToken ct);
    }
}
