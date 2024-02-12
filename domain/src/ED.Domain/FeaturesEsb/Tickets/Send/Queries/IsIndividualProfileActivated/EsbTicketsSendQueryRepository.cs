using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : IEsbTicketsSendQueryRepository
    {
        public async Task<bool> IsIndividualProfileActivatedAsync(
            int profileId,
            CancellationToken ct)
        {
            bool isActivated = await (
                from p in this.DbContext.Set<Profile>()

                where p.Id == profileId

                select (p.IsActivated && !p.IsPassive))
                .SingleAsync(ct);

            return isActivated;
        }
    }
}
