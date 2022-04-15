using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceProfilesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<GetAuthenticationInfoVO?> GetAuthenticationInfoAsync(
            string certificateThumbprint,
            string? operatorIdentifier,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(operatorIdentifier))
            {
                GetAuthenticationInfoVO? vo = await (
                    from l in this.DbContext.Set<Login>()

                    join p in this.DbContext.Set<Profile>()
                        on l.ElectronicSubjectId equals p.ElectronicSubjectId

                    where l.IsActive
                        && l.CertificateThumbprint == certificateThumbprint
                        && p.IsActivated

                    select new GetAuthenticationInfoVO(p.Id, l.Id, null))
                    .FirstOrDefaultAsync(ct);

                return vo;
            }
            else
            {
                GetAuthenticationInfoVO? vo = await (
                    from l in this.DbContext.Set<Login>()

                    join p in this.DbContext.Set<Profile>()
                        on l.ElectronicSubjectId equals p.ElectronicSubjectId

                    join lp in this.DbContext.Set<LoginProfile>()
                        on p.Id equals lp.ProfileId

                    join l2 in this.DbContext.Set<Login>()
                        on lp.LoginId equals l2.Id

                    join p2 in this.DbContext.Set<Profile>()
                        on l2.ElectronicSubjectId equals p2.ElectronicSubjectId

                    where l.IsActive
                        && l.CertificateThumbprint == certificateThumbprint
                        && p.IsActivated
                        && p2.Identifier == operatorIdentifier

                    select new GetAuthenticationInfoVO(p.Id, l.Id, l2.Id))
                    .FirstOrDefaultAsync(ct);

                return vo;
            }
        }
    }
}
