using System;
using System.Data;
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
        public async Task<GetTicketsVO[]> GetTicketsAsync(
            int adminUserId,
            DateTime @from,
            DateTime to,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, \"{from}\", \"{to}\") called",
                nameof(GetTicketsAsync),
                adminUserId,
                from,
                to);

            GetTicketsVO[] vos = await (
                from ts in this.DbContext.Set<TicketStat>()

                where ts.TicketStatDate > @from
                    && ts.TicketStatDate < to.AddDays(1)

                orderby ts.TicketStatDate

                select new GetTicketsVO(
                    ts.TicketStatDate,
                    ts.ReceivedTicketIndividuals,
                    ts.ReceivedPenalDecreeIndividuals,
                    ts.ReceivedTicketLegalEntites,
                    ts.ReceivedPenalDecreeLegalEntites,
                    ts.InternalServed,
                    ts.ExternalServed,
                    ts.Annulled,
                    ts.EmailNotifications,
                    ts.PhoneNotifications,
                    ts.DeliveredTicketIndividuals,
                    ts.DeliveredPenalDecreeIndividuals,
                    ts.DeliveredTicketLegalEntites,
                    ts.DeliveredPenalDecreeLegalEntites,
                    ts.SentToActiveProfiles,
                    ts.SentToPassiveProfiles))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
