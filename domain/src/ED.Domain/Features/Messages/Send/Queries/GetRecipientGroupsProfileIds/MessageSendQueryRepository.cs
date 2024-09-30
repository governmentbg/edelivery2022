using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<int[]> GetRecipientGroupsProfileIdsAsync(
            int[] recipientGroupIds,
            CancellationToken ct)
        {
            int[] profileIds = await (
                from rgp in this.DbContext.Set<RecipientGroupProfile>()

                join p in this.DbContext.Set<Profile>()
                    on rgp.ProfileId equals p.Id

                where p.IsActivated
                    && !p.HideAsRecipient
                    && this.DbContext.MakeIdsQuery(recipientGroupIds).Any(id => id.Id == rgp.RecipientGroupId)

                select p.Id)
                .ToArrayAsync(ct);

            return profileIds;
        }
    }
}
