using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<GetLoginPermissionsVO> GetLoginPermissionsAsync(
            int profileId,
            int loginId,
            CancellationToken ct)
        {
            var loginProfilePermissions = await (
                from l in this.DbContext.Set<Login>()

                join p in this.DbContext.Set<Profile>()
                    on l.ElectronicSubjectId equals p.ElectronicSubjectId

                join lpp in this.DbContext.Set<LoginProfilePermission>()
                                .Where(lpp1 => lpp1.ProfileId == profileId)
                    on l.Id equals lpp.LoginId
                    into lj1
                from lpp in lj1.DefaultIfEmpty()

                join t in this.DbContext.Set<Template>()
                    on lpp.TemplateId equals t.TemplateId
                    into lj2
                from t in lj2.DefaultIfEmpty()

                where l.Id == loginId

                select new
                {
                    LoginElectronicSubjectName = l.ElectronicSubjectName,
                    ProfileIdentifier = p.Identifier,
                    Permission = lpp != null ? lpp.Permission : (LoginProfilePermissionType?)null,
                    TemplateId = lpp != null ? lpp.TemplateId : null,
                    TemplateName = t != null ? t.Name : null
                })
                .ToArrayAsync(ct);

            GetLoginPermissionsVO vo = loginProfilePermissions
                .GroupBy(k => new
                {
                    k.LoginElectronicSubjectName,
                    k.ProfileIdentifier
                })
                .Select(e => new GetLoginPermissionsVO(
                    e.Key.LoginElectronicSubjectName,
                    e.Key.ProfileIdentifier,
                    e
                        .Where(e => e.Permission.HasValue)
                        .Select(g => new GetLoginPermissionsVOPermissions(
                            g.Permission!.Value,
                            g.TemplateId,
                            g.TemplateName))
                        .ToArray()))
                .Single();

            return vo;
        }
    }
}
