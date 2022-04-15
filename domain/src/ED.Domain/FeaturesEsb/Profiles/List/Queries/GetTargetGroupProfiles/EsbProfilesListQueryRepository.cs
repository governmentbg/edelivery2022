using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IEsbProfilesListQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<TableResultVO<GetTargetGroupProfilesVO>> GetTargetGroupProfilesAsync(
            int targetGroupId,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            TableResultVO<GetTargetGroupProfilesVO> vos = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where (p.IsActivated || (targetGroupId == TargetGroup.IndividualTargetGroupId && p.IsPassive))
                    && tg.TargetGroupId == targetGroupId

                orderby p.Id

                select new GetTargetGroupProfilesVO(
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
