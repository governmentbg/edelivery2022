using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetRecipientGroupsCountVO> GetRecipientGroupsCountAsync(
            int profileId,
            CancellationToken ct)
        {
            int vo = await (
                    from rg in this.DbContext.Set<RecipientGroup>()

                    where rg.ProfileId == profileId
                          && !rg.ArchiveDate.HasValue

                    select rg)
                .CountAsync(ct);

            return new GetRecipientGroupsCountVO(vo);
        }
    }
}
