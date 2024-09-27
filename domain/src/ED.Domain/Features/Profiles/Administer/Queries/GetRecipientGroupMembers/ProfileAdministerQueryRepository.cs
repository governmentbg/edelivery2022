using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<TableResultVO<GetRecipientGroupMembersVO>> GetRecipientGroupMembersAsync(
            int recipientGroupId,
            CancellationToken ct)
        {
            TableResultVO<GetRecipientGroupMembersVO> vos = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                join rgp in this.DbContext.Set<RecipientGroupProfile>()
                    on rg.RecipientGroupId equals rgp.RecipientGroupId

                join p in this.DbContext.Set<Profile>()
                    on rgp.ProfileId equals p.Id

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                where rg.RecipientGroupId == recipientGroupId

                select new GetRecipientGroupMembersVO(
                    p.Id,
                    p.ElectronicSubjectName,
                    tg.Name,
                    p.HideAsRecipient))
                .ToTableResultAsync(null, null, ct);

            return vos;
        }
    }
}
