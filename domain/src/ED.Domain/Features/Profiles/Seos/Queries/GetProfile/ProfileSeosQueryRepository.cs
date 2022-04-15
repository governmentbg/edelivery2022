using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileSeosQueryRepository;

namespace ED.Domain
{
    partial class ProfileSeosQueryRepository : IProfileSeosQueryRepository
    {
        public async Task<GetProfileVO> GetProfileAsync(
            int profileId,
            CancellationToken ct)
        {
            GetProfileVO vo = await (
                from p in this.DbContext.Set<Profile>()

                join a in this.DbContext.Set<Address>()
                    on p.AddressId equals a.AddressId
                    into lj1
                from a in lj1.DefaultIfEmpty()

                where p.Id == profileId

                select new GetProfileVO(
                    p.ElectronicSubjectName,
                    p.Identifier,
                    p.EmailAddress,
                    p.Phone,
                    a.Residence,
                    a.City))
                .SingleAsync(ct);

            return vo;
        }
    }
}
