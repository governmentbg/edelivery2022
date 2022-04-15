using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        Task<GetForwardedMessageAsRecipientVO> GetForwardedMessageAsRecipientAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
