using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminReportsListQueryRepository;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : IAdminReportsListQueryRepository
    {
        public async Task<GetNotificationsVO[]> GetNotificationsAsync(
            int adminUserId,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken ct)
        {
            // carried over from old project
            // TODO: should we have a better way to log audit actions?
            this.logger.LogInformation($"{nameof(GetNotificationsAsync)} ({adminUserId}, \"{fromDate}\", \"{toDate}\") called");

            Expression<Func<QueueMessage, bool>> predicate =
                PredicateBuilder.True<QueueMessage>();

            GetNotificationsVO[] vos = await (
                from q in this.DbContext.Set<QueueMessage>()

                where q.StatusDate >= fromDate
                      && q.StatusDate < toDate.AddDays(1)
                      && (q.Type == QueueMessageType.Email
                        || q.Type == QueueMessageType.Sms
                        || q.Type == QueueMessageType.SmsDeliveryCheck
                        || q.Type == QueueMessageType.Viber
                        || q.Type == QueueMessageType.ViberDeliveryCheck)
                      && (q.Status == QueueMessageStatus.Processed
                        || q.Status == QueueMessageStatus.Errored)

                group q by new
                {
                    q.Type,
                    q.StatusDate.Date
                }
                into g

                orderby g.Key.Date descending

                select new GetNotificationsVO(
                    g.Key.Type,
                    g.Count(m => m.Status == QueueMessageStatus.Processed),
                    g.Count(m => m.Status == QueueMessageStatus.Errored),
                    g.Key.Date
                ))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
