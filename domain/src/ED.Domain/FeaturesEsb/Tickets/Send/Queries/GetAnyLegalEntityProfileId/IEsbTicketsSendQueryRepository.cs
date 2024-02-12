using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<int?> GetAnyLegalEntityProfileIdAsync(
            string identifier,
            int[] targetGroupIds,
            CancellationToken ct);
    }
}
