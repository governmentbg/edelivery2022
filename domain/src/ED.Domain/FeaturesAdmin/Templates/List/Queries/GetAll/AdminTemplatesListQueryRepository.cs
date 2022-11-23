using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IAdminTemplatesListQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesListQueryRepository : IAdminTemplatesListQueryRepository
    {
        public async Task<TableResultVO<GetAllVO>> GetAllAsync(
            int offset,
            int limit,
            CancellationToken ct)
        {
            IQueryable<GetAllVO> query =
                from t in this.DbContext.Set<Template>()

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
        }
    }
}
