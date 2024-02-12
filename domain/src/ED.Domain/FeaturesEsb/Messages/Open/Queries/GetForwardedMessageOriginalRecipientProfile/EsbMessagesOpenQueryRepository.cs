using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbMessagesOpenQueryRepository : IEsbMessagesOpenQueryRepository
    {
        public async Task<int> GetForwardedMessageOriginalRecipientProfile(
            int messageId,
            int forwardedMessageId,
            CancellationToken ct)
        {
            int recipientProfileId = await (
                from fm in this.DbContext.Set<ForwardedMessage>()

                join m in this.DbContext.Set<Message>()
                    on fm.MessageId equals m.MessageId

                where fm.MessageId == messageId
                    && fm.ForwardedMessageId == forwardedMessageId

                select m.SenderProfileId)
                .SingleAsync(ct);

            return recipientProfileId;
        }
    }
}
