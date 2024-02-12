using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : IEsbTicketsSendQueryRepository
    {
        public async Task<int?> GetDuplicateTicketIdAsync(
            int senderProfileId,
            string? documentSeries,
            string documentNumber,
            DateTime issueDate,
            CancellationToken ct)
        {
            int? ticketId = await (
              from t in this.DbContext.Set<Ticket>()

              where t.SenderProfileId == senderProfileId
                && t.DocumentSeries == documentSeries
                && t.DocumentNumber == documentNumber
                && t.IssueDate.HasValue
                && t.IssueDate.Value.Year == issueDate.Year // todo: better way to compare date (sql) with datetime(c#) until ef core 8
                && t.IssueDate.Value.Month == issueDate.Month
                && t.IssueDate.Value.Day == issueDate.Day

              select t.MessageId)
              .Cast<int?>()
              .FirstOrDefaultAsync(ct);

            return ticketId;
        }
    }
}
