using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminRecipientGroupsCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminRecipientGroupsCreateEditViewQueryRepository : IAdminRecipientGroupsCreateEditViewQueryRepository
    {
        public async Task<GetRecipientGroupMembersVO[]> GetRecipientGroupMembersAsync(
            int recipientGroupId,
            CancellationToken ct)
        {
            GetRecipientGroupMembersVO[] vos = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                join rgp in this.DbContext.Set<RecipientGroupProfile>()
                    on rg.RecipientGroupId equals rgp.RecipientGroupId

                join p in this.DbContext.Set<Profile>()
                    on rgp.ProfileId equals p.Id

                where rg.RecipientGroupId == recipientGroupId

                select new GetRecipientGroupMembersVO(
                    p.Id,
                    p.ElectronicSubjectName))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
