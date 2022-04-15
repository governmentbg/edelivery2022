using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetIndividualVO> GetIndividualAsync(
            int profileId,
            CancellationToken ct)
        {
            GetIndividualVO vo = await (
                from p in this.DbContext.Set<Profile>()

                join i in this.DbContext.Set<Individual>()
                    on p.ElectronicSubjectId equals i.IndividualId

                join a in this.DbContext.Set<Address>()
                    on p.AddressId equals a.AddressId
                    into lj1
                from a in lj1.DefaultIfEmpty()

                where p.Id == profileId

                select new GetIndividualVO(
                    i.FirstName,
                    i.MiddleName,
                    i.LastName,
                    p.Identifier,
                    p.EmailAddress,
                    p.Phone,
                    a != null ? a.Residence : null))
                .SingleAsync(ct);

            return vo;
        }
    }
}
