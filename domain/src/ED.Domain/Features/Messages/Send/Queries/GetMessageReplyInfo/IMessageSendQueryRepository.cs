using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<GetMessageReplyInfoVO> GetMessageReplyInfoAsync(
            int messageId,
            CancellationToken ct);
    }
}
