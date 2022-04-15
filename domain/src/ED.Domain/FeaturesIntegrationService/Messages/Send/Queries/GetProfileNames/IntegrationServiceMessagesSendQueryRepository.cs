using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceMessagesSendQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceMessagesSendQueryRepository : IIntegrationServiceMessagesSendQueryRepository
    {
        public async Task<GetProfileNamesVO[]> GetProfileNamesAsync(
            int[] profileIds,
            CancellationToken ct)
        {
            GetProfileNamesVO[] vos = await (
                from p in this.DbContext.Set<Profile>()

                where this.DbContext.MakeIdsQuery(profileIds).Any(id => id.Id == p.Id)

                select new GetProfileNamesVO(
                    p.Id,
                    p.ElectronicSubjectName))
                .ToArrayAsync(ct);

            return vos;
        }
    }
}
