using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<TableResultVO<GetHistoryVO>> GetHistoryAsync(
            int profileId,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
