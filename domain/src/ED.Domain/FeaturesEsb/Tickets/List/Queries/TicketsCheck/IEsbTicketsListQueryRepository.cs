using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsListQueryRepository
    {
        Task<TicketsCheckVO[]> TicketsCheckAsync(
            int[] ticketIds,
            int? deliveryStatus,
            DateTime? from,
            DateTime? to,
            int profileId,
            CancellationToken ct);
    }
}
