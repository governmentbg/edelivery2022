using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageOpenQueryRepository;

namespace ED.Domain
{
    partial class MessageOpenQueryRepository : IMessageOpenQueryRepository
    {
        public async Task<GetSenderProfileVO> GetSenderProfileAsync(
            int messageId,
            CancellationToken ct)
        {
            GetSenderProfileVO vo = await (
                from m in this.DbContext.Set<Message>()

                join p in this.DbContext.Set<Profile>()
                    on m.SenderProfileId equals p.Id

                where m.MessageId == messageId

                select new GetSenderProfileVO(
                    p.Id,
                    p.EmailAddress,
                    p.ElectronicSubjectName,
                    p.Phone,
                    p.Identifier,
                    p.ProfileType))
                .SingleAsync(ct);

            return vo;
        }
    }
}
