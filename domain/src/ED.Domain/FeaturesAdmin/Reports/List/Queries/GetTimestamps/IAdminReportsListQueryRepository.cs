using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        Task<GetTimestampsVO> GetTimestampsAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct);
    }
}
