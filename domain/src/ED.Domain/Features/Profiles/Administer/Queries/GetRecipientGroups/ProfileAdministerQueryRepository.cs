using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<TableResultVO<GetRecipientGroupsVO>> GetRecipientGroupsAsync(
            int profileId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetRecipientGroupsVO> vos = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                join rgp in this.DbContext.Set<RecipientGroupProfile>()
                    on rg.RecipientGroupId equals rgp.RecipientGroupId
                    into lj1
                from rgp in lj1.DefaultIfEmpty()

                where rg.ProfileId == profileId
                    && !rg.ArchiveDate.HasValue

                group rgp by new
                {
                    rg.RecipientGroupId,
                    rg.Name,
                    rg.CreateDate,
                    rg.ModifyDate
                } into g

                orderby g.Key.Name

                select new GetRecipientGroupsVO(
                    g.Key.RecipientGroupId,
                    g.Key.Name,
                    g.Key.CreateDate,
                    g.Key.ModifyDate,
                    g.Count(e => e != null)))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
