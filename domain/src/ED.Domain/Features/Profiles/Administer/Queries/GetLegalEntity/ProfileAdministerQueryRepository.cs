using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<GetLegalEntityVO> GetLegalEntityAsync(
            int profileId,
            CancellationToken ct)
        {
            GetLegalEntityVO vo = await (
                from p in this.DbContext.Set<Profile>()

                join le in this.DbContext.Set<LegalEntity>()
                    on p.ElectronicSubjectId equals le.LegalEntityId

                join ple in this.DbContext.Set<LegalEntity>()
                    on le.ParentLegalEntityId equals ple.LegalEntityId
                    into lj1
                from ple in lj1.DefaultIfEmpty()

                join a in this.DbContext.Set<Address>()
                    on p.AddressId equals a.AddressId
                    into lj2
                from a in lj2.DefaultIfEmpty()

                where p.Id == profileId

                select new GetLegalEntityVO(
                    le.Name,
                    p.Identifier,
                    p.EmailAddress,
                    p.Phone,
                    a != null ? a.Residence : null,
                    ple != null ? ple.LegalEntityId.ToString() : null,
                    ple != null ? ple.Name : null))
                .SingleAsync(ct);

            return vo;
        }
    }
}
