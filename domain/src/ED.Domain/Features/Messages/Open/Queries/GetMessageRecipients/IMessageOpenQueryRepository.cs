using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetMessageRecipientsVO[]> GetMessageRecipientsAsync(
            int messageId,
            CancellationToken ct);
    }
}
