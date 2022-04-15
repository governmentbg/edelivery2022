using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetForwardedMessageAsRecipientVO> GetForwardedMessageAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
