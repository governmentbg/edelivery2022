using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static ED.Domain.IIntegrationServiceStatisticsListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceStatisticsListQueryRepository : IIntegrationServiceStatisticsListQueryRepository
    {
        private const int Days30 = 30;
        private const int Days10 = 10;

        public async Task<GetStatisticsVO> GetStatisticsAsync(
            CancellationToken ct)
        {
            this.logger.LogInformation(
                "{method} called",
                nameof(GetStatisticsAsync));

            int totalUsers = await (
                from l in this.DbContext.Set<Login>()

                where l.IsActive == true
                    && l.CertificateThumbprint == null

                select l)
                .CountAsync(ct);

            Dictionary<int, int> targetGroupsCount = await (
                from tg in this.DbContext.Set<TargetGroup>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on tg.TargetGroupId equals tgp.TargetGroupId
                    into lj1
                from tgp in lj1.DefaultIfEmpty()

                join p in this.DbContext.Set<Profile>()
                    on new
                    {
                        tgp.ProfileId,
                        IsActivated = true
                    }
                    equals new
                    {
                        ProfileId = p.Id,
                        p.IsActivated
                    }
                    into lj2
                from p in lj2.DefaultIfEmpty()

                where tg.ArchiveDate == null

                group p by new
                {
                    tg.TargetGroupId,
                    tg.Name
                }
                into g

                orderby g.Key.TargetGroupId

                select new
                {
                    g.Key.TargetGroupId,
                    Count = g.Count(e => e != null)
                })
                .ToDictionaryAsync(e => e.TargetGroupId, v => v.Count, ct);

            DateTime dateNow = DateTime.Now.Date;

            int totalMessages = await (
                from m in this.DbContext.Set<Message>()
                select m)
                .CountAsync(ct);

            int totalMessagesLast30Days = await (
                from m in this.DbContext.Set<Message>()

                where m.DateSent >= dateNow.AddDays(-Days30)

                select m)
                .CountAsync(ct);

            int totalMessagesLast10Days = await (
                from m in this.DbContext.Set<Message>()

                where m.DateSent >= dateNow.AddDays(-Days10)

                select m)
                .CountAsync(ct);

            int totalMessagesToday = await (
                from m in this.DbContext.Set<Message>()

                where m.DateSent >= dateNow

                select m)
                .CountAsync(ct);

            return new GetStatisticsVO(
                totalUsers,
                targetGroupsCount,
                totalMessages,
                totalMessagesLast30Days,
                totalMessagesLast10Days,
                totalMessagesToday);
        }
    }
}
