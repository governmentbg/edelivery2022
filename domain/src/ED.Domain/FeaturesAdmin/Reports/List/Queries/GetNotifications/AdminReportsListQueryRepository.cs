using System;
using System.Linq;
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
            this.logger.LogInformation(
                "{method} ({adminUserId}, \"{fromDate}\", \"{toDate}\") called",
                nameof(GetNotificationsAsync),
                adminUserId,
                fromDate,
                toDate);

            DateTime toDateModified = toDate.AddDays(1);

            GetNotificationsVO[] sms = await (
                from sd in this.DbContext.Set<SmsDelivery>()

                where sd.SentDate >= fromDate
                      && sd.SentDate < toDateModified

                group sd by new
                {
                    sd.SentDate.Date
                }
                into g

                orderby g.Key.Date descending

                select new GetNotificationsVO(
                    QueueMessageType.Sms,
                    g.Count(m => m.Status == DeliveryStatus.Delivered),
                    g.Count(m => m.Status == DeliveryStatus.Error),
                    g.Key.Date
                ))
                .ToArrayAsync(ct);

            GetNotificationsVO[] viber = await (
                from vd in this.DbContext.Set<ViberDelivery>()

                where vd.SentDate >= fromDate
                      && vd.SentDate < toDateModified

                group vd by new
                {
                    vd.SentDate.Date
                }
                into g

                orderby g.Key.Date descending

                select new GetNotificationsVO(
                    QueueMessageType.Viber,
                    g.Count(m => m.Status == DeliveryStatus.Delivered),
                    g.Count(m => m.Status == DeliveryStatus.Error),
                    g.Key.Date
                ))
                .ToArrayAsync(ct);

            GetNotificationsVO[] email = await (
                from ed in this.DbContext.Set<EmailDelivery>()

                where ed.SentDate >= fromDate
                      && ed.SentDate < toDateModified

                group ed by new
                {
                    ed.SentDate.Date
                }
                into g

                orderby g.Key.Date descending

                select new GetNotificationsVO(
                    QueueMessageType.Email,
                    g.Count(m => m.Status == DeliveryStatus.Delivered),
                    g.Count(m => m.Status == DeliveryStatus.Error),
                    g.Key.Date
                ))
                .ToArrayAsync(ct);

            GetNotificationsVO[] vos = sms
                .Concat(email)
                .Concat(viber)
                .ToArray();

            return vos;
        }
    }
}
