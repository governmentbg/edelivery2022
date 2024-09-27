using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        Task<GetTicketsVO[]> GetTicketsAsync(
            int adminUserId,
            DateTime from,
            DateTime to,
            CancellationToken ct);
    }
}
