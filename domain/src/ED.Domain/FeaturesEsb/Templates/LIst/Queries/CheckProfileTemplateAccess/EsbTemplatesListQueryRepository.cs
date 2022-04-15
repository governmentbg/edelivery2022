using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbTemplatesListQueryRepository : IEsbTemplatesListQueryRepository
    {
        public async Task<bool> CheckProfileTemplateAccessAsync(
            int profileId,
            int templateId,
            CancellationToken ct)
        {
            bool hasAccess = await (
                from p in this.DbContext.Set<Profile>()

                join tp in this.DbContext.Set<TemplateProfile>()
                    on p.Id equals tp.ProfileId

                where p.Id == profileId
                    && tp.TemplateId == templateId

                select tp.TemplateId)
                .Concat(
                    from p in this.DbContext.Set<Profile>()

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    join ttg in this.DbContext.Set<TemplateTargetGroup>()
                        on tgp.TargetGroupId equals ttg.TargetGroupId

                    where p.Id == profileId
                        && ttg.TemplateId == templateId

                    select ttg.TemplateId)
                .AnyAsync(ct);

            return hasAccess;
        }
    }
}
