using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<GetReplyInfoVO> GetInfoAsync(
            int messageId,
            CancellationToken ct);
    }
}
