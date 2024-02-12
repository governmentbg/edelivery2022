using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnumsNET;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IAdminReportsListQueryRepository;

namespace ED.Domain
{
    partial class AdminReportsListQueryRepository : IAdminReportsListQueryRepository
    {
        public async Task<GetTicketsVO> GetTicketsAsync(
            int adminUserId,
            DateTime from,
            DateTime to,
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method}({adminUserId}, \"{from}\", \"{to}\") called",
                nameof(GetTicketsAsync),
                adminUserId,
                from,
                to);

            int totalTickets = await (
                from t in this.DbContext.Set<Ticket>()

                select t.MessageId)
                .CountAsync(ct);

            string dailyTicketsQuery = $@"
    SELECT
        all_combinations.Type,
        all_combinations.TargetGroupId,
        COALESCE(aggregated_counts.MessageCount, 0) as MessageCount
    FROM
        (SELECT DISTINCT t.Type, tgp.TargetGroupId
        FROM Tickets t
        CROSS JOIN TargetGroupProfiles tgp
        where tgp.TargetGroupId in (1,2)) as all_combinations
    LEFT JOIN
        (SELECT
            t.Type,
            tgp.TargetGroupId,
            COUNT(m.MessageId) as MessageCount
        FROM Tickets t
        JOIN Messages m ON t.MessageId = m.MessageId
        JOIN MessageRecipients mr ON m.MessageId = mr.MessageId
        JOIN Profiles rp ON mr.ProfileId = rp.Id
        JOIN TargetGroupProfiles tgp ON rp.Id = tgp.ProfileId
        WHERE m.DateSent >= @from AND m.DateSent < @to
        GROUP BY t.Type, tgp.TargetGroupId) as aggregated_counts
    ON all_combinations.Type = aggregated_counts.Type
    AND all_combinations.TargetGroupId = aggregated_counts.TargetGroupId
";

            List<GetDailyTicketsQO> dailyTicketsQOs =
                await this.DbContext.Set<GetDailyTicketsQO>()
                    .FromSqlRaw(
                        dailyTicketsQuery,
                        new SqlParameter("from", SqlDbType.DateTime2)
                        {
                            Value = from
                        },
                        new SqlParameter("to", SqlDbType.DateTime2)
                        {
                            Value = to
                        })
                    .AsNoTracking()
                    .ToListAsync(ct);

            int dailyIndividualTickets = dailyTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && e.Type == TicketType.Ticket.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyLegalEntityTickets = dailyTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                    && e.Type == TicketType.Ticket.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyIndividualPenalDecrees = dailyTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && e.Type == TicketType.PenalDecree.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyLegalEntityPenalDecrees = dailyTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                    && e.Type == TicketType.PenalDecree.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyNotificationsByEmail = await (
                from ed in this.DbContext.Set<EmailDelivery>()

                where ed.Tag == "Tickets"
                    && ed.Status == 0
                    && ed.SentDate >= @from
                    && ed.SentDate < to

               select ed.EmailDeliveryId)
               .CountAsync(ct);

            int dailyNotificationsBySms = await (
                from sd in this.DbContext.Set<SmsDelivery>()

                where sd.Tag == "Tickets"
                    && sd.Status == 0
                    && sd.SentDate >= @from
                    && sd.SentDate < to

                select sd.SmsDeliveryId)
               .CountAsync(ct);

            int dailyNotificationsByViber = await (
                from vd in this.DbContext.Set<ViberDelivery>()

                where vd.Tag == "Tickets"
                    && vd.Status == 0
                    && vd.SentDate >= @from
                    && vd.SentDate < to

                select vd.ViberDeliveryId)
               .CountAsync(ct);

            string dailyReceivedTicketsQuery = $@"
    SELECT
        all_combinations.Type,
        all_combinations.TargetGroupId,
        COALESCE(aggregated_counts.MessageCount, 0) as MessageCount
    FROM
        (SELECT DISTINCT t.Type, tgp.TargetGroupId
         FROM Tickets t
         CROSS JOIN TargetGroupProfiles tgp
         where tgp.TargetGroupId in (1,2)) as all_combinations
    LEFT JOIN
        (SELECT
            t.Type,
            tgp.TargetGroupId,
            COUNT(m.MessageId) as MessageCount
         FROM Tickets t
         JOIN Messages m ON t.MessageId = m.MessageId
         JOIN MessageRecipients mr ON m.MessageId = mr.MessageId
         JOIN Profiles rp ON mr.ProfileId = rp.Id
         JOIN TargetGroupProfiles tgp ON rp.Id = tgp.ProfileId
         WHERE mr.DateReceived >= @from AND mr.DateReceived < @to
         GROUP BY t.Type, tgp.TargetGroupId) as aggregated_counts
    ON all_combinations.Type = aggregated_counts.Type 
       AND all_combinations.TargetGroupId = aggregated_counts.TargetGroupId
";

            List<GetDailyTicketsQO> dailyReceivedTicketsQOs =
                await this.DbContext.Set<GetDailyTicketsQO>()
                    .FromSqlRaw(
                        dailyReceivedTicketsQuery,
                        new SqlParameter("from", SqlDbType.DateTime2)
                        {
                            Value = from
                        },
                        new SqlParameter("to", SqlDbType.DateTime2)
                        {
                            Value = to
                        })
                    .AsNoTracking()
                    .ToListAsync(ct);

            int dailyReceivedIndividualTickets = dailyReceivedTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && e.Type == TicketType.Ticket.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyReceivedLegalEntityTickets = dailyReceivedTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                    && e.Type == TicketType.Ticket.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyReceivedIndividualPenalDecrees = dailyReceivedTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && e.Type == TicketType.PenalDecree.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;

            int dailyReceivedLegalEntityPenalDecrees = dailyReceivedTicketsQOs
                .FirstOrDefault(e => e.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                    && e.Type == TicketType.PenalDecree.AsString(EnumFormat.Description))?
                .MessageCount
                    ?? 0;


            int dailyPassiveProfiles = await (
                from p in this.DbContext.Set<Profile>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on p.Id equals mr.ProfileId

                join t in this.DbContext.Set<Ticket>()
                    on mr.MessageId equals t.MessageId

                join m in this.DbContext.Set<Message>()
                    on t.MessageId equals m.MessageId

                where p.IsPassive
                    && !p.IsActivated
                    && p.DateCreated >= @from
                    && p.DateCreated < to
                    && m.DateSent >= @from
                    && m.DateSent < to

                select p.Id)
               .CountAsync(ct);

            int dailyActiveProfiles = await (
                from p in this.DbContext.Set<Profile>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on p.Id equals mr.ProfileId

                join t in this.DbContext.Set<Ticket>()
                    on mr.MessageId equals t.MessageId

                join m in this.DbContext.Set<Message>()
                    on t.MessageId equals m.MessageId

                where p.IsActivated
                    && p.DateCreated >= @from
                    && p.DateCreated < to
                    && m.DateSent >= @from
                    && m.DateSent < to

                select p.Id)
               .CountAsync(ct);

            return new GetTicketsVO(
                totalTickets,
                dailyIndividualTickets,
                dailyLegalEntityTickets,
                dailyIndividualPenalDecrees,
                dailyLegalEntityPenalDecrees,
                dailyNotificationsByEmail,
                dailyNotificationsBySms + dailyNotificationsByViber,
                dailyReceivedIndividualTickets,
                dailyReceivedLegalEntityTickets,
                dailyReceivedIndividualPenalDecrees,
                dailyReceivedLegalEntityPenalDecrees,
                dailyPassiveProfiles,
                dailyActiveProfiles);
        }
    }
}
