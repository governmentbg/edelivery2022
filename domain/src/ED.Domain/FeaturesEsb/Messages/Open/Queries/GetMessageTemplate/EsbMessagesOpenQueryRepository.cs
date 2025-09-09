using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbMessagesOpenQueryRepository : IEsbMessagesOpenQueryRepository
    {
        public async Task<int?> GetMessageTemplateAsync(
            int messageId,
            CancellationToken ct)
        {
            int? templateId = await (
                from m in this.DbContext.Set<Message>()

                where m.MessageId == messageId

                select m.TemplateId)
                .FirstAsync(ct);

            return templateId;
        }
    }
}
