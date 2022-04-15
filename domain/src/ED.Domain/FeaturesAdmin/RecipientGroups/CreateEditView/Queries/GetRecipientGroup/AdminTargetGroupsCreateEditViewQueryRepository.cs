using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminRecipientGroupsCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminRecipientGroupsCreateEditViewQueryRepository : IAdminRecipientGroupsCreateEditViewQueryRepository
    {
        public async Task<GetRecipientGroupVO> GetRecipientGroupAsync(
            int recipientGroupId,
            CancellationToken ct)
        {
            GetRecipientGroupVO vo = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                where rg.RecipientGroupId == recipientGroupId

                select new GetRecipientGroupVO(
                    rg.RecipientGroupId,
                    rg.Name,
                    rg.CreateDate,
                    rg.ModifyDate,
                    rg.ArchiveDate))
                .SingleAsync(ct);

            return vo;
        }
    }
}
