using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminReportsListQueryRepository
    {
        Task<GetStatisticsVO> GetStatisticsAsync(
            int adminUserId,
            CancellationToken ct);
    }
}
