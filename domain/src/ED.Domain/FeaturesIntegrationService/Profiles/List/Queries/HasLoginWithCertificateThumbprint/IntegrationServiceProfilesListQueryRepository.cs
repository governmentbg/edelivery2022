using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<bool> HasLoginWithCertificateThumbprintAsync(
            string certificateThumbprint,
            CancellationToken ct)
        {
            return await (
                from l in this.DbContext.Set<Login>()

                join lp in this.DbContext.Set<LoginProfile>()
                    on l.Id equals lp.LoginId

                join p in this.DbContext.Set<Profile>()
                    on lp.ProfileId equals p.Id

                where l.IsActive
                    && l.CertificateThumbprint == certificateThumbprint
                    && p.IsActivated

                select l.Id
            ).AnyAsync(ct);
        }
    }
}
