using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository
    {
        public async Task<GetForwardMessageInfoVO> GetForwardMessageInfoAsync(
            int messageId,
            CancellationToken ct)
        {
            GetForwardMessageInfoVO vo = await (
                from m in this.DbContext.Set<Message>()

                where m.MessageId == messageId

                select new GetForwardMessageInfoVO(
                    m.Subject,
                    m.Orn)
                ).SingleAsync(ct);

            return vo;
        }
    }
}
