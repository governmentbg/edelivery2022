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
                    && targetGroupIds.Contains(tg.TargetGroupId)

                group tg by tg.TargetGroupId into g

                select new
                {
                    TargetGroupId = g.Key,
                    Value = g.Count()
                })
                .ToArrayAsync(ct);

            GetStatisticsVO vo = new(
                counts[0].Value,
                counts[1].Value,
                counts[2].Value);

            return vo;
        }
    }
}
