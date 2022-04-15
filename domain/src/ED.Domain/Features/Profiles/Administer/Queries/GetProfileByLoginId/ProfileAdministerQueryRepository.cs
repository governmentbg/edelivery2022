using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<int> GetProfileByLoginIdAsync(
            int loginId,
            CancellationToken ct)
        {
            int profileId = await (
                    from p in this.DbContext.Set<Profile>()

                    join l in this.DbContext.Set<Login>()

                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                    where l.Id == loginId

                    select p.Id)
                    .SingleOrDefaultAsync(ct);

            return profileId;
        }
    }
}
