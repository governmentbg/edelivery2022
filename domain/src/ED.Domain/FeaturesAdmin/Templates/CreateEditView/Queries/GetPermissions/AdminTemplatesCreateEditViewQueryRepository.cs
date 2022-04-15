using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTemplatesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesCreateEditViewQueryRepository : IAdminTemplatesCreateEditViewQueryRepository
    {
        public async Task<GetPermissionsVO> GetPermissionsAsync(
            int templateId,
            CancellationToken ct)
        {
            GetPermissionsVOProfile[] templateProfiles = await (
                from t in this.DbContext.Set<Template>()

                join tp in this.DbContext.Set<TemplateProfile>()
                    on t.TemplateId equals tp.TemplateId

                join p in this.DbContext.Set<Profile>()
                    on tp.ProfileId equals p.Id

                where t.TemplateId == templateId

                select new GetPermissionsVOProfile(
                    t.TemplateId,
                    tp.ProfileId,
                    p.ElectronicSubjectName,
                    tp.CanSend,
                    tp.CanReceive))
                .ToArrayAsync(ct);

            GetPermissionsVOTargetGroup[] templateTargetGroups = await (
                from t in this.DbContext.Set<Template>()

                join ttg in this.DbContext.Set<TemplateTargetGroup>()
                    on t.TemplateId equals ttg.TemplateId

                join tg in this.DbContext.Set<TargetGroup>()
                    on ttg.TargetGroupId equals tg.TargetGroupId

                where t.TemplateId == templateId

                select new GetPermissionsVOTargetGroup(
                    t.TemplateId,
                    ttg.TargetGroupId,
                    tg.Name,
                    ttg.CanSend,
                    ttg.CanReceive))
                .ToArrayAsync(ct);

            GetPermissionsVO vo = new(
                templateProfiles,
                templateTargetGroups);

            return vo;
        }
    }
}
