using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileListQueryRepository;

namespace ED.Domain
{
    partial class ProfileListQueryRepository : IProfileListQueryRepository
    {
        public async Task<GetStatisticsVO> GetStatisticsAsync(
            CancellationToken ct)
        {
            int[] targetGroupIds = new int[]
            {
                TargetGroup.LegalEntityTargetGroupId,
                TargetGroup.PublicAdministrationTargetGroupId,
                TargetGroup.SocialOrganizationTargetGroupId
            };

            var counts = await (
                from tg in this.DbContext.Set<TargetGroup>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on tg.TargetGroupId equals tgp.TargetGroupId

                join p in this.DbContext.Set<Profile>()
                    on tgp.ProfileId equals p.Id

                where p.IsActivated
                    && !p.IsPassive
                    && (!p.HideAsRecipient || tgp.TargetGroupId == TargetGroup.LegalEntityTargetGroupId)
                    && targetGroupIds.Contains(tg.TargetGroupId)

                group tg by tg.TargetGroupId into g

                select new
                {
                    TargetGroupId = g.Key,
                    Value = g.Count()
                })
                .ToArrayAsync(ct);

            int legalEntites = counts.FirstOrDefault(e => e.TargetGroupId == TargetGroup.LegalEntityTargetGroupId)?.Value ?? 0;
            int administrations = counts.FirstOrDefault(e => e.TargetGroupId == TargetGroup.PublicAdministrationTargetGroupId)?.Value ?? 0;
            int organizations = counts.FirstOrDefault(e => e.TargetGroupId == TargetGroup.SocialOrganizationTargetGroupId)?.Value ?? 0;

            GetStatisticsVO vo = new(
                legalEntites,
                administrations,
                organizations);

            return vo;
        }
    }
}
