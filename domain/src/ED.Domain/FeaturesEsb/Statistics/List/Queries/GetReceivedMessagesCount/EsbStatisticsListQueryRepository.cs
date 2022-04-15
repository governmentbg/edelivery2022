using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbStatisticsListQueryRepository : IEsbStatisticsListQueryRepository
    {
        public async Task<int> GetReceivedMessagesCountAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct)
        {
            int count = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join m in this.DbContext.Set<Message>()
                    on mr.MessageId equals m.MessageId

                where mr.DateReceived >= fromDate
                    && mr.DateReceived < toDate.AddDays(1)

                group m by m.MessageId into g

                select g.Key)
                .CountAsync(ct);

            return count;
        }
    }
}
