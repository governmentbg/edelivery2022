using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IJobsMessagesOpenQueryRepository;

namespace ED.Domain
{
    partial class JobsMessagesOpenQueryRepository : IJobsMessagesOpenQueryRepository
    {
        public async Task<GetTemplateContentVO> GetTemplateContentAsync(
            int templateId,
            CancellationToken ct)
        {
            GetTemplateContentVO vo = await (
                from t in this.DbContext.Set<Template>()

                where t.TemplateId == templateId

                select new GetTemplateContentVO(
                    t.TemplateId,
                    t.Content))
                .SingleAsync(ct);

            return vo;
        }
    }
}
