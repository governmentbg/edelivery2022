using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<string[]> GetRecipientGroupsNamesAsync(
            int[] recipientGroupIds,
            CancellationToken ct)
        {
            string[] names = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                where this.DbContext.MakeIdsQuery(recipientGroupIds).Any(id => id.Id == rg.RecipientGroupId)

                select rg.Name)
                .ToArrayAsync(ct);

            return names;
        }
    }
}
