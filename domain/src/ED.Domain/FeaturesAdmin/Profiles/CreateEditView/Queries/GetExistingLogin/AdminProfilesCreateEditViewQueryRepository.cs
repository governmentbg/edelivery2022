using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<bool> GetExistingLoginAsync(
            Guid loginSubjectId,
            CancellationToken ct)
        {
            bool existingLogin = await (
                from l in this.DbContext.Set<Login>()

                where l.ElectronicSubjectId == loginSubjectId

                select l.Id)
                .AnyAsync(ct);

            return existingLogin;
        }
    }
}
