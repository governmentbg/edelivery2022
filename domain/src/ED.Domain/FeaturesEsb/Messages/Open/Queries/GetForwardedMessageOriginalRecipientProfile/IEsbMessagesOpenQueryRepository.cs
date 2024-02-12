using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        Task<int> GetForwardedMessageOriginalRecipientProfile(
            int messageId,
            int forwardedMessageId,
            CancellationToken ct);
    }
}
