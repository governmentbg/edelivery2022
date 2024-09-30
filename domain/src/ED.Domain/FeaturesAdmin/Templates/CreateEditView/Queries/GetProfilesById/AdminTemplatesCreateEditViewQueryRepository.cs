using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminTemplatesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminTemplatesCreateEditViewQueryRepository : IAdminTemplatesCreateEditViewQueryRepository
    {
        public async Task<GetProfilesByIdVO[]> GetProfilesByIdAsync(
            int[] ids,
            CancellationToken ct)
        {
            GetProfilesByIdVO[] vos = await (
                from p in this.DbContext.Set<Profile>()

                where this.DbContext.MakeIdsQuery(ids).Any(id => id.Id == p.Id)

                orderby p.Identifier, p.ElectronicSubjectName

                select new GetProfilesByIdVO(
                    p.Id,
                    $"{p.Identifier} - {p.ElectronicSubjectName}"))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
