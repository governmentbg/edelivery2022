using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageListQueryRepository;

namespace ED.Domain
{
    partial class MessageListQueryRepository : IMessageListQueryRepository
    {
        public async Task<GetForwardHistoryVO[]> GetForwardHistoryAsync(
            int messageId,
            CancellationToken ct)
        {
            GetForwardHistoryVO[] first = await (
                from m in this.DbContext.Set<Message>()

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                where m.MessageId == messageId

                select new GetForwardHistoryVO(
                    m.MessageId,
                    p1.ElectronicSubjectName,
                    m.RecipientsAsText,
                    m.DateSent!.Value,
                    null))
                .ToArrayAsync(ct);

            GetForwardHistoryVO[] forwardings = await (
                from fm in this.DbContext.Set<ForwardedMessage>()

                join m in this.DbContext.Set<Message>()
                    on fm.MessageId equals m.MessageId

                join p1 in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p1.Id

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join p2 in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p2.Id

                where fm.ForwardedMessageId == messageId

                select new GetForwardHistoryVO(
                    m.MessageId,
                    p1.ElectronicSubjectName,
                    p2.ElectronicSubjectName,
                    m.DateSent!.Value,
                    mr.DateReceived))
                .ToArrayAsync(ct);

            return first.Concat(forwardings).ToArray();
        }
    }
}
