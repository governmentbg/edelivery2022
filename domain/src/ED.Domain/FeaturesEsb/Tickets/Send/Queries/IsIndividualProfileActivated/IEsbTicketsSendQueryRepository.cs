using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<bool> IsIndividualProfileActivatedAsync(
            int profileId,
            CancellationToken ct);
    }
}
