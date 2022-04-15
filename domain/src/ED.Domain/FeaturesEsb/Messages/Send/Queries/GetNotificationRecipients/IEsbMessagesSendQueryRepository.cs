using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesSendQueryRepository
    {
        Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsAsync(
            int messageId,
            CancellationToken ct);
    }
}
