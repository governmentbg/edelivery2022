using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IJobsDataPortalListQueryRepository;

namespace ED.Domain
{
    partial class JobsDataPortalListQueryRepository : IJobsDataPortalListQueryRepository
    {
        public async Task<GetProfilesMonthlyStatisticsVO[]> GetProfilesMonthlyStatisticsAsync(
            DateTime @from,
            DateTime to,
            CancellationToken ct)
        {
            GetProfilesMonthlyStatisticsVO individuals =
                await this.GetIndividualProfilesMonthlyStatisticsAsync(
                    from,
                    to,
                    ct);

            GetProfilesMonthlyStatisticsVO legalEntites =
                await this.GetLegalEntityProfilesMonthlyStatisticsAsync(
                    from,
                    to,
                    ct);

            GetProfilesMonthlyStatisticsVO[] others =
                await this.GetOthersProfilesMonthlyStatisticsAsync(
                    from,
                    to,
                    ct);

            return others.Concat(
                new GetProfilesMonthlyStatisticsVO[]
                {
                    legalEntites,
                    individuals
                })
                .ToArray();
        }

        private async Task<GetProfilesMonthlyStatisticsVO> GetIndividualProfilesMonthlyStatisticsAsync(
            DateTime @from,
            DateTime to,
            CancellationToken ct)
        {
            // individuals
            int sent = await (
                from m in this.DbContext.Set<Message>()

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && m.DateSent > @from
                    && m.DateSent < to

                select m.MessageId)
                .CountAsync(ct);

            int received = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && mr.DateReceived > @from
                    && mr.DateReceived < to

                select mr.MessageId)
                .CountAsync(ct);

            return new GetProfilesMonthlyStatisticsVO(
                "Физически лица",
                "Общо",
                sent,
                received);
        }

        private async Task<GetProfilesMonthlyStatisticsVO> GetLegalEntityProfilesMonthlyStatisticsAsync(
            DateTime @from,
            DateTime to,
            CancellationToken ct)
        {
            // legal entities
            int sent = await (
                from m in this.DbContext.Set<Message>()

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where tgp.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                    && m.DateSent > @from
                    && m.DateSent < to

                select m.MessageId)
                .CountAsync(ct);

            int received = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where tgp.TargetGroupId == TargetGroup.LegalEntityTargetGroupId
                    && mr.DateReceived > @from
                    && mr.DateReceived < to

                select mr.MessageId)
                .CountAsync(ct);

            return new GetProfilesMonthlyStatisticsVO(
                "Юридически лица",
                "Общо",
                sent,
                received);
        }

        private async Task<GetProfilesMonthlyStatisticsVO[]> GetOthersProfilesMonthlyStatisticsAsync(
            DateTime @from,
            DateTime to,
            CancellationToken ct)
        {
            // others
            var all = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where p.IsActivated
                    && tg.TargetGroupId != TargetGroup.IndividualTargetGroupId
                    && tg.TargetGroupId != TargetGroup.LegalEntityTargetGroupId

                select new
                {
                    ProfileId = p.Id,
                    ProfileName = p.ElectronicSubjectName,
                    TargetGroupName = tg.Name,
                })
                .ToArrayAsync(ct);

            var sent = await (
                from m in this.DbContext.Set<Message>()

                join p in this.DbContext.Set<Profile>()
                on m.SenderProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where p.IsActivated
                    && tgp.TargetGroupId != TargetGroup.IndividualTargetGroupId
                    && tgp.TargetGroupId != TargetGroup.LegalEntityTargetGroupId
                    && m.DateSent > @from
                    && m.DateSent < to

                group new { m } by new { p.Id } into g

                orderby g.Key.Id

                select new
                {
                    ProfileId = g.Key.Id,
                    Count = g.Count(),
                })
                .ToArrayAsync(ct);

            var received = await (
                from mr in this.DbContext.Set<MessageRecipient>()


                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where p.IsActivated
                    && tgp.TargetGroupId != TargetGroup.IndividualTargetGroupId
                    && tgp.TargetGroupId != TargetGroup.LegalEntityTargetGroupId
                    && mr.DateReceived > @from
                    && mr.DateReceived < to

                group new { mr } by new { p.Id } into g

                orderby g.Key.Id

                select new
                {
                    ProfileId = g.Key.Id,
                    Count = g.Count(),
                })
                .ToArrayAsync(ct);

            GetProfilesMonthlyStatisticsVO[] others = (
                from a in all

                join s in sent
                    on a.ProfileId equals s.ProfileId
                    into lj1
                from s in lj1.DefaultIfEmpty()

                join r in received
                    on a.ProfileId equals r.ProfileId
                    into lj2
                from r in lj2.DefaultIfEmpty()

                orderby a.TargetGroupName, a.ProfileName
                select new GetProfilesMonthlyStatisticsVO(
                    a.TargetGroupName,
                    a.ProfileName,
                    s?.Count ?? 0,
                    r?.Count ?? 0))
                .ToArray();

            return others;
        }
    }
}
