using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<bool> IsRecipientProfileIndividualAsync(
            int messageId,
            CancellationToken ct);
    }
}
