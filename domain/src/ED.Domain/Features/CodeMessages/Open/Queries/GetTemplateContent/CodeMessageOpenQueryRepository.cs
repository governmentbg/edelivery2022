using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class CodeMessageOpenQueryRepository : ICodeMessageOpenQueryRepository
    {
        public async Task<string> GetTemplateContentAsync(
            int templateId,
            CancellationToken ct)
        {
            string content = await (
                from t in this.DbContext.Set<Template>()

                where t.TemplateId == templateId

                select t.Content)
                .SingleAsync(ct);

            return content;
        }
    }
}
