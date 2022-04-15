using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ITemplateListQueryRepository;

namespace ED.Domain
{
    partial class TemplateListQueryRepository : ITemplateListQueryRepository
    {
        public async Task<GetContentVO> GetContentAsync(
            int templateId,
            CancellationToken ct)
        {
            GetContentVO vo = await (
                from t in this.DbContext.Set<Template>()

                where t.TemplateId == templateId

                select new GetContentVO(
                    t.TemplateId,
                    t.Content))
                .FirstAsync(ct);

            return vo;
        }
    }
}
