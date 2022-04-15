using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        Task<TableResultVO<GetFreeBlobsVO>> GetFreeBlobsAsync(
            int profileId,
            int offset,
            int limit,
            string fileName,
            string author,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct);
    }
}
