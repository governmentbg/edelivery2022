using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminNomenclaturesListQueryRepository;

namespace ED.Domain
{
    partial class AdminNomenclaturesListQueryRepository : IAdminNomenclaturesListQueryRepository
    {
        public async Task<GetTargetGroupsByIdVO[]> GetTargetGroupsByIdAsync(
            int[] ids,
            CancellationToken ct)
        {
            return await (
                from tg in this.DbContext.Set<TargetGroup>()

                where this.DbContext.MakeIdsQuery(ids).Any(id => id.Id == tg.TargetGroupId)

                orderby tg.TargetGroupId

                select new GetTargetGroupsByIdVO(
                   tg.TargetGroupId,
                   tg.Name))
                .ToArrayAsync(ct);
        }
    }
}
