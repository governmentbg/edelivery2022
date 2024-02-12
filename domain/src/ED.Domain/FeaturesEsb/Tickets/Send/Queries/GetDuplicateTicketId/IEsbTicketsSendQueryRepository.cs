using System;
using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTicketsSendQueryRepository
    {
        Task<int?> GetDuplicateTicketIdAsync(
            int senderProfileId,
            string? documentSeries,
            string documentNumber,
            DateTime issueDate,
            CancellationToken ct);
    }
}
