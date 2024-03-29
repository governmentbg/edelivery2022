using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        Task<GetEFormsVO[]> GetEFormsAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            string eFormServiceNumber,
            CancellationToken ct);
    }
}
