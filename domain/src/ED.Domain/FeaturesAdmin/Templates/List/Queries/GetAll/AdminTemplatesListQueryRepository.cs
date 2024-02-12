using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IAdminTemplatesListQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesListQueryRepository : IAdminTemplatesListQueryRepository
    {
        public async Task<TableResultVO<GetAllVO>> GetAllAsync(
            string? term,
            TemplateStatus templateStatus,
            int offset,
            int limit,
            CancellationToken ct)
        {
            Expression<Func<Template, bool>> predicate =
                BuildTemplatePredicate(templateStatus, term);

            IQueryable<GetAllVO> query =
                from t in this.DbContext.Set<Template>().Where(predicate)

                join rlsl in this.DbContext.Set<LoginSecurityLevel>()
                    on t.ReadLoginSecurityLevelId equals rlsl.LoginSecurityLevelId

                join wlsl in this.DbContext.Set<LoginSecurityLevel>()
                    on t.WriteLoginSecurityLevelId equals wlsl.LoginSecurityLevelId

                orderby t.CreateDate descending

                select new GetAllVO(
                    t.TemplateId,
                    t.Name,
                    t.IdentityNumber,
                    t.Category,
                    t.CreateDate,
                    t.PublishDate,
                    t.ArchiveDate,
                    t.IsSystemTemplate,
                    rlsl.Name,
                    wlsl.Name);

            TableResultVO<GetAllVO> table =
                await query.ToTableResultAsync(offset, limit, ct);

            return table;

            Expression<Func<Template, bool>> BuildTemplatePredicate(
                TemplateStatus templateStatus,
                string? term)
            {
                Expression<Func<Template, bool>> predicate =
                    PredicateBuilder.True<Template>();

                if (!string.IsNullOrEmpty(term))
                {
                    predicate = predicate
                        .And(e => e.Name.Contains(term)
                            || e.IdentityNumber.Contains(term));
                }

                switch (templateStatus)
                {

                    case TemplateStatus.Draft:
                        predicate = predicate.And(t => t.PublishDate == null);
                        break;
                    case TemplateStatus.Published:
                        predicate = predicate.And(t => t.PublishDate != null && t.ArchiveDate == null);
                        break;
                    case TemplateStatus.Archived:
                        predicate = predicate.And(t => t.ArchiveDate != null);
                        break;

                    default:
                        break;
                }

                return predicate;
            }
        }
    }
}
