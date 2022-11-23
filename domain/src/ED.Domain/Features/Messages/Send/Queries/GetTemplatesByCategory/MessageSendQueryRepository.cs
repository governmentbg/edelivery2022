using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IMessageSendQueryRepository;

namespace ED.Domain
{
    partial class MessageSendQueryRepository : IMessageSendQueryRepository
    {
        public async Task<TableResultVO<GetTemplatesByCategoryVO>> GetTemplatesByCategoryAsync(
            int profileId,
            int loginId,
            string? category,
            CancellationToken ct)
        {
            IQueryable<int> templateIdsByCategoryQuery =
                (from p in this.DbContext.Set<Profile>()
                 join tgp in this.DbContext.Set<TargetGroupProfile>()
                     on p.Id equals tgp.ProfileId
                 join ttg in this.DbContext.Set<TemplateTargetGroup>()
                     on tgp.TargetGroupId equals ttg.TargetGroupId
                 join t in this.DbContext.Set<Template>()
                    on ttg.TemplateId equals t.TemplateId
                 where p.Id == profileId && t.Category == category
                 select ttg.TemplateId
                ).Union(
                    from tp in this.DbContext.Set<TemplateProfile>()
                    join t in this.DbContext.Set<Template>()
                        on tp.TemplateId equals t.TemplateId
                    where tp.ProfileId == profileId && t.Category == category
                    select tp.TemplateId
                );

            var permissions = await this.DbContext.Set<LoginProfilePermission>()
                .Where(lp =>
                    lp.LoginId == loginId &&
                    lp.ProfileId == profileId)
                .Select(lp =>
                    new
                    {
                        lp.Permission,
                        lp.TemplateId
                    })
                .ToArrayAsync(ct);

            if (!permissions.Any(p =>
                    p.Permission == LoginProfilePermissionType.OwnerAccess ||
                    p.Permission == LoginProfilePermissionType.FullMessageAccess))
            {
                int[] writeProfileMessageAccessTemplateIds = permissions
                    .Where(p => p.Permission == LoginProfilePermissionType.WriteProfileMessageAccess)
                    .Select(p => p.TemplateId!.Value)
                    .ToArray();

                if (writeProfileMessageAccessTemplateIds.Length == 0)
                {
                    return TableResultVO.Empty<GetTemplatesByCategoryVO>();
                }

                templateIdsByCategoryQuery =
                    from tId in templateIdsByCategoryQuery
                    where this.DbContext.MakeIdsQuery(writeProfileMessageAccessTemplateIds)
                            .Any(id => id.Id == tId)
                    select tId;
            }

            TableResultVO<GetTemplatesByCategoryVO> table =
                await (
                    from t in this.DbContext.Set<Template>()
                    where t.ArchiveDate == null &&
                        templateIdsByCategoryQuery.Any(atId => t.TemplateId == atId)
                    select new GetTemplatesByCategoryVO(
                        t.TemplateId,
                        t.Name)
                ).ToTableResultAsync(null, null, ct);

            return table;
        }
    }
}
