using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbTargetGroupsListQueryRepository;

namespace ED.Domain
{
    partial class EsbTargetGroupsListQueryRepository : IEsbTargetGroupsListQueryRepository
    {
        public async Task<GetTargetGroupsVO[]> GetTargetGroupsAsync(
            int profileId,
            CancellationToken ct)
        {
            var targetGroups = await (
                from tg in this.DbContext.Set<TargetGroup>()

                where tg.ArchiveDate == null

                orderby tg.TargetGroupId

                select new
                {
                    tg.TargetGroupId,
                    tg.Name
                })
                .ToArrayAsync(ct);

            var accessibleTargetGroups = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tgm in this.DbContext.Set<TargetGroupMatrix>()
                    on tgp.TargetGroupId equals tgm.SenderTargetGroupId

                where p.Id == profileId

                select tgm.RecipientTargetGroupId)
                .ToArrayAsync(ct);

            GetTargetGroupsVO[] vos = targetGroups
                .Select(e => new GetTargetGroupsVO(
                    e.TargetGroupId,
                    e.Name,
                    accessibleTargetGroups.Contains(e.TargetGroupId)))
                .ToArray();

            return vos;
        }
    }
}
