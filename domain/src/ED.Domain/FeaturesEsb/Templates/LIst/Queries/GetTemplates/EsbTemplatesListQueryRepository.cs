using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using static ED.Domain.IEsbTemplatesListQueryRepository;

namespace ED.Domain
{
    partial class EsbTemplatesListQueryRepository : IEsbTemplatesListQueryRepository
    {
        public async Task<GetTemplatesVO[]> GetTemplatesAsync(
            int profileId,
            CancellationToken ct)
        {
            var templates = await (
                from t in this.DbContext.Set<Template>()

                where (t.PublishDate != null && t.ArchiveDate == null)
                    || t.TemplateId == Template.TicketTemplate // ticket template is not published and this is a bypass

                orderby t.TemplateId

                select new
                {
                    t.TemplateId,
                    t.Name,
                    t.IdentityNumber,
                    t.ReadLoginSecurityLevelId,
                    t.WriteLoginSecurityLevelId,
                    t.ResponseTemplateId,
                })
                .ToArrayAsync(ct);

            var accessibleTemplates = await (
                from p in this.DbContext.Set<Profile>()

                join tp in this.DbContext.Set<TemplateProfile>()
                    on p.Id equals tp.ProfileId

                where p.Id == profileId

                select tp.TemplateId)
                .Concat(
                    from p in this.DbContext.Set<Profile>()

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    join ttg in this.DbContext.Set<TemplateTargetGroup>()
                        on tgp.TargetGroupId equals ttg.TargetGroupId

                    where p.Id == profileId

                    select ttg.TemplateId)
                .ToArrayAsync(ct);

            GetTemplatesVO[] vos = templates
                .Where(e => accessibleTemplates.Contains(e.TemplateId))
                .Select(e => new GetTemplatesVO(
                    e.TemplateId,
                    e.Name,
                    e.IdentityNumber,
                    e.ReadLoginSecurityLevelId,
                    e.WriteLoginSecurityLevelId,
                    e.ResponseTemplateId))
                .ToArray();

            return vos;
        }
    }
}
