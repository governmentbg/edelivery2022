using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbStatisticsListQueryRepository;

namespace ED.Domain
{
    partial class EsbStatisticsListQueryRepository : IEsbStatisticsListQueryRepository
    {
        public async Task<GetMessagesCountVO> GetMessagesCountAsync(
            DateTime month,
            CancellationToken ct)
        {
            int sent = await (
                from m in this.DbContext.Set<Message>()

                where m.DateSent >= month
                    && m.DateSent < month.AddMonths(1)

                select m)
                .CountAsync(ct);

            int received = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join m in this.DbContext.Set<Message>()
                    on mr.MessageId equals m.MessageId

                where mr.DateReceived >= month
                    && mr.DateReceived < month.AddMonths(1)

                group m by m.MessageId into g

                select g.Key)
                .CountAsync(ct);

            return new GetMessagesCountVO(sent, received);
        }
    }
}
