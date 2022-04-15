using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesSendQueryRepository
    {
        Task<GetTemporaryOrStorageBlobsVO[]> GetTemporaryOrStorageBlobsAsync(
            int profileId,
            int[] blobIds,
            CancellationToken ct);
    }
}
