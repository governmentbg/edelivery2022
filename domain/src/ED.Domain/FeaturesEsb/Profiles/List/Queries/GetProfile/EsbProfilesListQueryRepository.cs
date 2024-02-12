using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesListQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesListQueryRepository : IEsbProfilesListQueryRepository
    {
        public async Task<GetProfileVO?> GetProfileAsync(
            int profileId,
            CancellationToken ct)
        {
            GetProfileVO? vo = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                join a in this.DbContext.Set<Address>()
                    on p.AddressId equals a.AddressId
                    into lj1
                    from a in lj1.DefaultIfEmpty()

                where p.Id == profileId

                orderby p.Id

                select new GetProfileVO(
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone,
                    a != null
                        ? new GetProfileVOAddress(
                            a.AddressId,
                            a.Residence,
                            a.City,
                            a.State,
                            a.Country)
                        : null))
                .SingleOrDefaultAsync(ct);

            return vo;
        }
    }
}
