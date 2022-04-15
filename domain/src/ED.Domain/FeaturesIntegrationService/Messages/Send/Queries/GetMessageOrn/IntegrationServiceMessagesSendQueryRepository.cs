using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<string?> GetMessageOrnAsync(
            int messageId,
            int recipientProfileId,
            CancellationToken ct)
        {
            string identifier = await (
                from m in this.DbContext.Set<Message>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                where m.MessageId == messageId
                    && mr.ProfileId == recipientProfileId

                select m.Orn)
                .SingleAsync(ct);

            return identifier;
        }
    }
}
