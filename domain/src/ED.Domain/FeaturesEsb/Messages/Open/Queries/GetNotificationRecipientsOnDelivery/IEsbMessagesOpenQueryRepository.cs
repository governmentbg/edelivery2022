using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        Task<GetNotificationRecipientsOnDeliveryVO[]> GetNotificationRecipientsOnDeliveryAsync(
            int messageId,
            CancellationToken ct);
    }
}
