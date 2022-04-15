using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<string> GetProfileNameAsync(
            int profileId,
            CancellationToken ct)
        {
            string name = await (
                from p in this.DbContext.Set<Profile>()

                where p.Id == profileId

                select p.ElectronicSubjectName)
                .SingleAsync(ct);

            return name;
        }
    }
}
