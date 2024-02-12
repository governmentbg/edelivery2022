using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<GetNotificationRecipientsVO[]> GetNotificationRecipientsAsync(
            bool isRecipientIndividual,
            int messageId,
            CancellationToken ct);
    }
}
