using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class TicketsOpenQueryRepository : ITicketsOpenQueryRepository
    {
        public async Task<string> GetProfileIdentifierAsync(
            int profileId,
            CancellationToken ct)
        {
            string profileIdentifier = await (
                from p in this.DbContext.Set<Profile>()

                where p.Id == profileId

                select p.Identifier)
                .SingleAsync(ct);

            return profileIdentifier;
        }
    }
}
