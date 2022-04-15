using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<TableResultVO<GetTargetGroupsVO>> GetTargetGroupsAsync(
            int profileId,
            CancellationToken ct)
        {
            TableResultVO<GetTargetGroupsVO> vos = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tgm in this.DbContext.Set<TargetGroupMatrix>()
                    on tgp.TargetGroupId equals tgm.SenderTargetGroupId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgm.RecipientTargetGroupId equals tg.TargetGroupId

                where p.Id == profileId
                    && tg.ArchiveDate == null

                select new GetTargetGroupsVO(
                    tg.TargetGroupId,
                    tg.Name))
                .ToTableResultAsync(null, null, ct);

            return vos;
        }
    }
}
