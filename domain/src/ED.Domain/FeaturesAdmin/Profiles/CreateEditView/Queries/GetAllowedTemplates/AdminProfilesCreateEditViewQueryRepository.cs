using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetAllowedTemplatesVO[]> GetAllowedTemplatesAsync(
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

            GetAllowedTemplatesVO[] table =
                await (
                    from t in this.DbContext.Set<Template>()
                    where t.ArchiveDate == null
                        && t.PublishDate != null
                        && allowedTemplateIdsQuery.Any(atId => t.TemplateId == atId)
                    select new GetAllowedTemplatesVO(
                        t.TemplateId,
                        t.Name)
                ).ToArrayAsync(ct);

            return table;
        }
    }
}
