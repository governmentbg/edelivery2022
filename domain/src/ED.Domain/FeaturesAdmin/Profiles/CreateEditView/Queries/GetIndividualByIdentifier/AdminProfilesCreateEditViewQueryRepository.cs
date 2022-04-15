using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetIndividualByIdentifierVO?> GetIndividualByIdentifierAsync(
            string identifier,
            CancellationToken ct)
        {
            GetIndividualByIdentifierVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join i in this.DbContext.Set<Individual>()
                    on p.ElectronicSubjectId equals i.IndividualId

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                where p.IsActivated
                    && EF.Functions.Like(p.Identifier, identifier)

                select new GetIndividualByIdentifierVO(
                    l.Id,
                    l.ElectronicSubjectName))
                .FirstOrDefaultAsync(ct);

            return vo;
        }
    }
}
