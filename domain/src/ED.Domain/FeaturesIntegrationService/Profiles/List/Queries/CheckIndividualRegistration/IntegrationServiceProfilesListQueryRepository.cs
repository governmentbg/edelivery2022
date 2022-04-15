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
        public async Task<CheckIndividualRegistrationVO> CheckIndividualRegistrationAsync(
            string identifier,
            CancellationToken ct)
        {
            var profile = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && p.IsActivated
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select new
                {
                    p.Identifier,
                    p.ElectronicSubjectId,
                    p.ElectronicSubjectName
                })
                .SingleOrDefaultAsync(ct);

            if (profile != null)
            {
                int[] targetGroups = new int[]
                {
                    TargetGroup.LegalEntityTargetGroupId,
                    TargetGroup.PublicAdministrationTargetGroupId,
                    TargetGroup.SocialOrganizationTargetGroupId,
                };

                var loginProfiles = await (
                    from l in this.DbContext.Set<Login>()

                    join lp in this.DbContext.Set<LoginProfile>()
                        on l.Id equals lp.LoginId

                    join p in this.DbContext.Set<Profile>()
                        on lp.ProfileId equals p.Id

                    join tgp in this.DbContext.Set<TargetGroupProfile>()
                        on p.Id equals tgp.ProfileId

                    where l.ElectronicSubjectId == profile.ElectronicSubjectId
                        && targetGroups.Contains(tgp.TargetGroupId)
                        && p.IsActivated

                    select new
                    {
                        p.ElectronicSubjectName,
                        p.Identifier,
                        tgp.TargetGroupId,
                    })
                    .ToArrayAsync(ct);

                return new CheckIndividualRegistrationVO(
                    identifier,
                    true,
                    profile.ElectronicSubjectName,
                    loginProfiles
                        .Select(e => new CheckIndividualRegistrationVOProfile(
                            e.ElectronicSubjectName,
                            e.Identifier,
                            e.TargetGroupId))
                        .ToArray());
            }
            else
            {
                return new CheckIndividualRegistrationVO(
                    identifier,
                    false,
                    null,
                    Array.Empty<CheckIndividualRegistrationVOProfile>());
            }
        }
    }
}
