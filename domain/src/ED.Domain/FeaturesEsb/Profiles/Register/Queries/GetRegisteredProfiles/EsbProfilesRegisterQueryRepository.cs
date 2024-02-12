using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesRegisterQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<TableResultVO<GetRegisteredProfilesVO>> GetRegisteredProfilesAsync(
            string identifier,
            int? offset,
            int? limit,
            CancellationToken ct)
        {
            TableResultVO<GetRegisteredProfilesVO> vos = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join tg in this.DbContext.Set<TargetGroup>()
                    on tgp.TargetGroupId equals tg.TargetGroupId

                join a in this.DbContext.Set<Address>()
                    on p.AddressId equals a.AddressId
                    into lj1
                from a in lj1.DefaultIfEmpty()

                where EF.Functions.Like(p.Identifier, identifier)
                    && (p.IsActivated || p.IsPassive)
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select new GetRegisteredProfilesVO(
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone,
                    a != null
                        ? new GetRegisteredProfilesVOAddress(
                            a.AddressId,
                            a.Residence,
                            a.City,
                            a.State,
                            a.Country)
                        : null,
                    p.IsActivated,
                    p.IsPassive,
                    tg.TargetGroupId,
                    tg.Name))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
