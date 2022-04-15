using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminAdministratorsCreateEditViewQueryRepository : IAdminAdministratorsCreateEditViewQueryRepository
    {
        public async Task<bool> HasExistingAdministratorAsync(
            string userName,
            CancellationToken ct)
        {
            bool hasExistingAdministrator = await (
                from au in this.DbContext.Set<AdminUser>()

                where au.NormalizedUserName == userName.ToUpperInvariant()

                select au
            ).AnyAsync(ct);


            return hasExistingAdministrator;
        }
    }
}
