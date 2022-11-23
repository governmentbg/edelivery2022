using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbBlobsListQueryRepository
    {
        Task<TableResultVO<GetStorageBlobsVO>> GetStorageBlobsAsync(
            int profileId,
            int? offset,
            int? limit,
            CancellationToken ct);
    }
}
