using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<bool> CheckLoginSendOnBehalfOfAsync(
            int loginId,
            CancellationToken ct)
        {
            bool checkLoginSendOnBehalfOf = await (
                from l in this.DbContext.Set<Login>()

                where l.Id == loginId

                select (l.CanSendOnBehalfOf ?? false))
                .SingleAsync(ct);

            return checkLoginSendOnBehalfOf;
        }
    }
}
