using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ED.Domain.IProfileAdministerQueryRepository;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<TableResultVO<GetHistoryVO>> GetHistoryAsync(
            int profileId,
            int offset,
            int limit,
            CancellationToken ct)
        {
            TableResultVO<GetHistoryVO> vos = await (
                from ph in this.DbContext.Set<ProfilesHistory>()

                join l in this.DbContext.Set<Login>()
                    on ph.ActionLogin equals l.Id
                    into lj1
                from l in lj1.DefaultIfEmpty()

                join ap in this.DbContext.Set<AdminsProfile>()
                    on ph.ActionByAdminUserId equals ap.Id
                    into lj2
                from ap in lj2.DefaultIfEmpty()

                where ph.ProfileId == profileId

                orderby ph.ActionDate descending

                select new GetHistoryVO(
                    ph.Id,
                    ph.ProfileId,
                    ph.ActionDate,
                    ph.Action,
                    l != null
                        ? l.ElectronicSubjectName
                        : null,
                    ph.ActionDetails,
                    ap != null
                        ? $"{ap.FirstName} {ap.LastName}"
                        : null,
                    ph.IPAddress))
                .ToTableResultAsync(offset, limit, ct);

            return vos;
        }
    }
}
