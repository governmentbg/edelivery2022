using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<GetMessageSenderVO?> GetMessageSenderAsync(
            int messageId,
            int recipientProfileId,
            CancellationToken ct)
        {
            GetMessageSenderVO? vo = await (
                from m in this.DbContext.Set<Message>()

                join mr in this.DbContext.Set<MessageRecipient>()
                    on m.MessageId equals mr.MessageId

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                join fm in this.DbContext.Set<ForwardedMessage>()
                    on m.MessageId equals fm.MessageId
                    into lj1
                from fm in lj1.DefaultIfEmpty()

                join m2 in this.DbContext.Set<Message>()
                    on fm.ForwardedMessageId equals m2.MessageId
                    into lj2
                from m2 in lj2.DefaultIfEmpty()

                join p2 in this.DbContext.Set<Profile>()
                    on m2.SenderProfileId equals p2.Id
                    into lj3
                from p2 in lj3.DefaultIfEmpty()

                where m.MessageId == messageId
                    && mr.ProfileId == recipientProfileId

                select new GetMessageSenderVO(
                    p2 != null ? p2.Id : p.Id,
                    p2 != null
                        ? p2.ElectronicSubjectName
                        : p.ElectronicSubjectName))
                .SingleOrDefaultAsync(ct);

            return vo;
        }
    }
}
