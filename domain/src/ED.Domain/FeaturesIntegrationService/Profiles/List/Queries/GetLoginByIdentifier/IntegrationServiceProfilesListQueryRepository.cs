using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceProfilesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<GetLoginByIdentifierVO?> GetLoginByIdentifierAsync(
            string identifier,
            CancellationToken ct)
        {
            GetLoginByIdentifierVO? vo = (await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join l in this.DbContext.Set<Login>()
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                join lp in this.DbContext.Set<LoginProfile>()
                    on l.Id equals lp.LoginId

                join p2 in this.DbContext.Set<Profile>()
                    on lp.ProfileId equals p2.Id

                join tgp2 in this.DbContext.Set<TargetGroupProfile>()
                    on p2.Id equals tgp2.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && p.IsActivated
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId
                    && l.IsActive
                    && p2.IsActivated

                select new
                {
                    LoginId = l.Id,
                    LoginSubjectId = l.ElectronicSubjectId.ToString(),
                    LoginName = l.ElectronicSubjectName,
                    LoginEmail = l.Email,
                    LoginPhone = l.PhoneNumber,
                    LoginIsActive = l.IsActive,
                    l.CertificateThumbprint,
                    l.PushNotificationsUrl,
                    ProfileId = p2.Id,
                    ProfileIsDefault = lp.IsDefault,
                    ProfileSubjectId = p2.ElectronicSubjectId,
                    ProfileName = p2.ElectronicSubjectName,
                    ProfileEmail = p2.EmailAddress,
                    ProfilePhone = p2.Phone,
                    p2.ProfileType,
                    p2.DateCreated,
                    tgp2.TargetGroupId,
                })
                .ToArrayAsync(ct))
                .GroupBy(e => new
                {
                    e.LoginId,
                    e.LoginSubjectId,
                    e.LoginName,
                    e.LoginEmail,
                    e.LoginPhone,
                    e.LoginIsActive,
                    e.CertificateThumbprint,
                    e.PushNotificationsUrl,
                })
                .Select(e => new GetLoginByIdentifierVO(
                    e.Key.LoginId,
                    e.Key.LoginSubjectId,
                    e.Key.LoginName,
                    e.Key.LoginEmail,
                    e.Key.LoginPhone,
                    e.Key.LoginIsActive,
                    e.Key.CertificateThumbprint,
                    e.Key.PushNotificationsUrl,
                    e
                        .Select(p => new GetLoginByIdentifierVOProfiles(
                            p.ProfileId,
                            p.ProfileIsDefault,
                            p.ProfileSubjectId.ToString(),
                            p.ProfileName,
                            p.ProfileEmail,
                            p.ProfilePhone,
                            p.ProfileType,
                            p.DateCreated,
                            p.TargetGroupId))
                        .ToArray()
                    ))
                .FirstOrDefault();

            return vo;
        }
    }
}
