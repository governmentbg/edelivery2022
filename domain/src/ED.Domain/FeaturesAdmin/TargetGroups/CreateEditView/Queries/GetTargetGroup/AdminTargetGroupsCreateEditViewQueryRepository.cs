using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTargetGroupsCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTargetGroupsCreateEditViewQueryRepository : IAdminTargetGroupsCreateEditViewQueryRepository
    {
        public async Task<GetTargetGroupVO> GetTargetGroupAsync(
            int targetGroupId,
            CancellationToken ct)
        {
            GetTargetGroupVO vo = await (
                from tg in this.DbContext.Set<TargetGroup>()

                where tg.TargetGroupId == targetGroupId

                select new GetTargetGroupVO(
                    tg.TargetGroupId,
                    tg.Name,
                    tg.CreateDate,
                    tg.ModifyDate,
                    tg.ArchiveDate))
                .SingleAsync(ct);

            return vo;
        }
    }
}
