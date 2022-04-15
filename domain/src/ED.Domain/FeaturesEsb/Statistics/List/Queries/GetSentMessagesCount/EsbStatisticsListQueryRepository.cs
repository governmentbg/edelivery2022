using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbStatisticsListQueryRepository : IEsbStatisticsListQueryRepository
    {
        public async Task<int> GetSentMessagesCountAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct)
        {
            int count = await (
                from m in this.DbContext.Set<Message>()

                where m.DateSent.HasValue
                    && m.DateSent >= fromDate
                    && m.DateSent < toDate.AddDays(1)

                select m)
                .CountAsync(ct);

            return count;
        }
    }
}
