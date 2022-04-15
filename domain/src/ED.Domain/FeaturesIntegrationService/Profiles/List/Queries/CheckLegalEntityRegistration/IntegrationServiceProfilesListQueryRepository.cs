using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IIntegrationServiceProfilesListQueryRepository;

namespace ED.Domain
{
    partial class IntegrationServiceProfilesListQueryRepository : IIntegrationServiceProfilesListQueryRepository
    {
        public async Task<CheckLegalEntityRegistrationVO> CheckLegalEntityRegistrationAsync(
            string identifier,
            CancellationToken ct)
        {
            var profile = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && p.IsActivated // only active and that excludes profiles with passive registration
                    && tgp.TargetGroupId == TargetGroup.LegalEntityTargetGroupId

                select new
                {
                    p.Id,
                    p.Identifier,
                    p.ElectronicSubjectId,
                    p.ElectronicSubjectName,
                    p.EmailAddress,
                    p.Phone
                })
                .SingleOrDefaultAsync(ct);

            if (profile != null)
            {
                var loginProfiles = await (
                    from lp in this.DbContext.Set<LoginProfile>()

                    join l in this.DbContext.Set<Login>()
                        on lp.LoginId equals l.Id

                    join p in this.DbContext.Set<Profile>()
                        on l.ElectronicSubjectId equals p.ElectronicSubjectId

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    where lp.ProfileId == profile.Id
                        && l.IsActive
                        && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                    select new
                    {
                        l.ElectronicSubjectName,
                        p.Identifier,
                        tgp.TargetGroupId,
                    })
                    .ToArrayAsync(ct);

                return new CheckLegalEntityRegistrationVO(
                    identifier,
                    true,
                    profile.ElectronicSubjectName,
                    profile.Phone,
                    profile.EmailAddress,
                    loginProfiles
                        .Select(e => new CheckLegalEntityRegistrationVOLogin(
                            e.ElectronicSubjectName,
                            e.Identifier,
                            e.TargetGroupId))
                        .ToArray());
            }
            else
            {
                return new CheckLegalEntityRegistrationVO(
                    identifier,
                    false,
                    null,
                    null,
                    null,
                    Array.Empty<CheckLegalEntityRegistrationVOLogin>());
            }
        }
    }
}
