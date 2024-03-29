using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetPassiveProfileDataVO> GetPassiveProfileDataAsync(
            int loginId,
            CancellationToken ct)
        {
            GetPassiveProfileDataVO vo = await (
                from l in this.DbContext.Set<Login>()

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                join i in this.DbContext.Set<Individual>()
                    on p.ElectronicSubjectId equals i.IndividualId

                join a in this.DbContext.Set<Address>()
                    on p.AddressId equals a.AddressId

                where l.Id == loginId

                select new GetPassiveProfileDataVO(
                    i.FirstName,
                    i.MiddleName,
                    i.LastName,
                    a.Residence,
                    p.EmailAddress,
                    p.Phone))
                .SingleAsync(ct);

            return vo;
        }
    }
}
