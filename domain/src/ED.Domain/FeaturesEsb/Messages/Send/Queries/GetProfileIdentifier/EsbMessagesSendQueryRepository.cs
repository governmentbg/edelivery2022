using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbMessagesSendQueryRepository : IEsbMessagesSendQueryRepository
    {
        public async Task<string> GetProfileIdentifierAsync(
            int profileId,
            CancellationToken ct)
        {
            string identifier = await (
                from p in this.DbContext.Set<Profile>()

                where p.Id == profileId

                select p.Identifier)
                .SingleAsync(ct);

            return identifier;
        }
    }
}
