using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        Task<TableResultVO<GetMyBlobsVO>> GetMyBlobsAsync(
            int profileId,
            long? maxFileSize,
            string[]? allowedFileTypes,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
