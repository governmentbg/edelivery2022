using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IAdminRecipientGroupsListQueryRepository;

namespace ED.Domain
{
    partial class AdminRecipientGroupsListQueryRepository : IAdminRecipientGroupsListQueryRepository
    {
        public async Task<TableResultVO<GetAllVO>> GetAllAsync(
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetAllVO> vos = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                where rg.IsPublic

                orderby rg.CreateDate descending

                select new GetAllVO(
                    rg.RecipientGroupId,
                    rg.Name,
                    rg.CreateDate,
                    rg.ArchiveDate))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
