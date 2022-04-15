using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetBlobTimestampVO> GetBlobTimestampAsync(
            string accessCode,
            int blobId,
            CancellationToken ct);
    }
}
