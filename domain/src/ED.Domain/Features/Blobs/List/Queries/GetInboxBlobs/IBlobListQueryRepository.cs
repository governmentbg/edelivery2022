using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        Task<TableResultVO<GetInboxBlobsVO>> GetInboxBlobsAsync(
            int profileId,
            int offset,
            int limit,
            string fileName,
            string? messageSubject,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken ct);
    }
}
