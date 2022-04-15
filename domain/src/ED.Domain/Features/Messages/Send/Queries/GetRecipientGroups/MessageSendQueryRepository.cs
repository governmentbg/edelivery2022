using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<TableResultVO<GetRecipientGroupsVO>> GetRecipientGroupsAsync(
            int profileId,
            CancellationToken ct)
        {
            TableResultVO<GetRecipientGroupsVO> vos = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                where (rg.ProfileId == profileId || rg.IsPublic)
                    && rg.ArchiveDate == null
                    && this.DbContext.Set<RecipientGroupProfile>().Any(r => r.RecipientGroupId == rg.RecipientGroupId)

                select new GetRecipientGroupsVO(
                    rg.RecipientGroupId,
                    $"{(rg.IsPublic ? ": " : string.Empty)}{rg.Name}"))
                .ToTableResultAsync(null, null, ct);

            return vos;
        }
    }
}
