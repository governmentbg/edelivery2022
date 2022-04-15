using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetRecipientGroupVO> GetRecipientGroupAsync(
            int recipientGroupId,
            int profileId,
            CancellationToken ct)
        {
            GetRecipientGroupVO vo = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                where rg.RecipientGroupId == recipientGroupId
                    && rg.ProfileId == profileId
                    && !rg.ArchiveDate.HasValue

                select new GetRecipientGroupVO(
                    rg.RecipientGroupId,
                    rg.Name,
                    rg.CreateDate,
                    rg.ModifyDate))
                .SingleAsync(ct);

            return vo;
        }
    }
}
