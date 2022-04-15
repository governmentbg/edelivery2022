using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesListQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<GetTargetGroupProfileByIdentifierVO?> GetTargetGroupProfileByIdentifierAsync(
            int targetGroupId,
            string identifier,
            CancellationToken ct)
        {
            GetTargetGroupProfileByIdentifierVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where (p.IsActivated || (targetGroupId == TargetGroup.IndividualTargetGroupId && p.IsPassive))
                    && tg.TargetGroupId == targetGroupId
                    && EF.Functions.Like(p.Identifier, identifier)

                orderby p.IsActivated

                select new GetTargetGroupProfileByIdentifierVO(
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
