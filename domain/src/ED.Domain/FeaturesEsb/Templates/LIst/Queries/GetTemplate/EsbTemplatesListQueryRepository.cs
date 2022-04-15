using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbTemplatesListQueryRepository;

namespace ED.Domain
{
    partial class EsbTemplatesListQueryRepository : IEsbTemplatesListQueryRepository
    {
        public async Task<GetTemplateVO> GetTemplateAsync(
            int templateId,
            CancellationToken ct)
        {
            GetTemplateVO vo = await (
                from t in this.DbContext.Set<Template>()

                where t.TemplateId == templateId

                select new GetTemplateVO(
                    t.TemplateId,
                    t.Name,
                    t.IdentityNumber,
                    t.ReadLoginSecurityLevelId,
                    t.WriteLoginSecurityLevelId,
                    t.Content,
                    t.ResponseTemplateId))
                .SingleAsync(ct);

            return vo;
        }
    }
}
