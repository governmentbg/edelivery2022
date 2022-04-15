using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<bool> ExistsTemplateAsync(
            int templateId,
            CancellationToken ct)
        {
            return await this.DbContext.Set<Template>()
                .AnyAsync(t => t.TemplateId == templateId, ct);
        }
    }
}
