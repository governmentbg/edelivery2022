using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<TableResultVO<GetAllowedTemplatesVO>> GetAllowedTemplatesAsync(
            int profileId,
            CancellationToken ct)
        {
            IQueryable<int> allowedTemplateIdsQuery =
                (from p in this.DbContext.Set<Profile>()
                 join tgp in this.DbContext.Set<TargetGroupProfile>()
                     on p.Id equals tgp.ProfileId
                 join ttg in this.DbContext.Set<TemplateTargetGroup>()
                     on tgp.TargetGroupId equals ttg.TargetGroupId
                 where p.Id == profileId
                 select ttg.TemplateId
                ).Union(
                    from tp in this.DbContext.Set<TemplateProfile>()
                    where tp.ProfileId == profileId
                    select tp.TemplateId
                );

            TableResultVO<GetAllowedTemplatesVO> table =
                await (
                    from t in this.DbContext.Set<Template>()
                    where t.ArchiveDate == null &&
                        allowedTemplateIdsQuery.Any(atId => t.TemplateId == atId)
                    select new GetAllowedTemplatesVO(
                        t.TemplateId,
                        t.Name)
                ).ToTableResultAsync(null, null, ct);

            return table;
        }
    }
}
