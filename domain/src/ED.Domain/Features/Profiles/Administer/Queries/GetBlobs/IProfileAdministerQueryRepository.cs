using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<TableResultVO<GetBlobsVO>> GetBlobsAsync(
            int profileId,
            CancellationToken ct);
    }
}
