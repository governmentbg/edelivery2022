using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbBlobsListQueryRepository
    {
        Task<CheckStorageBlobVO> CheckStorageBlobAsync(
            int profileId,
            int blobId,
            CancellationToken ct);
    }
}
