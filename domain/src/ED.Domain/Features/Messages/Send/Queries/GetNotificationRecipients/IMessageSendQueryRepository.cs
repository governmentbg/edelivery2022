using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsAsync(
            int messageId,
            CancellationToken ct);
    }
}
