using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        public async Task<GetMessageRecipientsVO[]> GetMessageRecipientsAsync(
            int messageId,
            CancellationToken ct)
        {
            GetMessageRecipientsVO[] vos = await (
                from mr in this.DbContext.Set<MessageRecipient>()

                join p in this.DbContext.Set<Profile>()
                    on mr.ProfileId equals p.Id

                where mr.MessageId == messageId

                select new GetMessageRecipientsVO(
                    p.ElectronicSubjectName,
                    mr.DateReceived))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
