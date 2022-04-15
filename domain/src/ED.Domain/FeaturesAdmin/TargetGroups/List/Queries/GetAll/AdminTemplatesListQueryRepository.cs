using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IAdminTargetGroupsListQueryRepository;

namespace ED.Domain
{
    partial class AdminTargetGroupsListQueryRepository : IAdminTargetGroupsListQueryRepository
    {
        public async Task<TableResultVO<GetAllVO>> GetAllAsync(
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetAllVO> vos = await (
                from tg in this.DbContext.Set<TargetGroup>()

                orderby tg.CreateDate descending

                select new GetAllVO(
                    tg.TargetGroupId,
                    tg.Name,
                    tg.CreateDate,
                    tg.ArchiveDate))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
