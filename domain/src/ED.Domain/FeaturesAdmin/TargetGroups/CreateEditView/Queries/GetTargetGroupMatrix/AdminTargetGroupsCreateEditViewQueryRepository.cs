using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTargetGroupsCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTargetGroupsCreateEditViewQueryRepository : IAdminTargetGroupsCreateEditViewQueryRepository
    {
        public async Task<GetTargetGroupMatrixVO[]> GetTargetGroupMatrixAsync(
            int targetGroupId,
            CancellationToken ct)
        {
            GetTargetGroupMatrixVO[] vos = await (
                from tgm in this.DbContext.Set<TargetGroupMatrix>()

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgm.RecipientTargetGroupId equals tg.TargetGroupId


                where tgm.SenderTargetGroupId == targetGroupId

                select new GetTargetGroupMatrixVO(
                    tg.TargetGroupId,
                    tg.Name))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
