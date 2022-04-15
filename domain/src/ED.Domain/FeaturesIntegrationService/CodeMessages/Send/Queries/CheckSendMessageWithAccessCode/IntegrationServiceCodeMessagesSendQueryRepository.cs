using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceCodeMessagesSendQueryRepository : IIntegrationServiceCodeMessagesSendQueryRepository
    {
        public async Task<bool> CheckSendMessageWithAccessCodeAsync(
            int profileId,
            CancellationToken ct)
        {
            bool checkSendMessageWithAccessCode = await (
                from p in this.DbContext.Set<Profile>()

                where p.Id == profileId

                select p.EnableMessagesWithCode ?? false)
                .SingleAsync(ct);

            return checkSendMessageWithAccessCode;
        }
    }
}
