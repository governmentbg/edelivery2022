using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<int?> GetIndividualProfileIdAsync(
            string identifier,
            CancellationToken ct);
    }
}
