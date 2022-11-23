using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.IEsbProfilesAuthenticateQueryRepository;

namespace ED.Domain
{
    partial class EsbProfilesAuthenticateQueryRepository : IEsbProfilesAuthenticateQueryRepository
    {
        public async Task<GetEsbUserVO?> GetEsbUserAsync(
            string oId,
            string clientId,
            string? operatorIdentifier,
            string? representedIdentifier,
            CancellationToken ct)
        {
            var profileLogin = await (
                from peu in this.DbContext.Set<ProfileEsbUser>()

                join p in this.DbContext.Set<Profile>()
                    on peu.ProfileId equals p.Id

                join l in this.DbContext.Set<Login>() // TODO: get integration login
                    on p.ElectronicSubjectId equals l.ElectronicSubjectId

                join lp in this.DbContext.Set<LoginProfile>()
                    on new { ProfileId = p.Id, LoginId = l.Id } equals new { lp.ProfileId, lp.LoginId }

                    // TODO https://github.com/dotnet/efcore/issues/26634
#pragma warning disable CS8604 // Possible null reference argument.
                where EF.Functions.Like(peu.OId, oId)
                    && EF.Functions.Like(peu.ClientId, clientId)
#pragma warning restore CS8604 // Possible null reference argument.
                    && p.IsActivated
                    && lp.IsDefault

                select new { ProfileId = p.Id, LoginId = l.Id })
                .FirstOrDefaultAsync(ct);

            if (profileLogin == null)
            {
                return null;
            }

            int? operatorLoginId = null;
            if (!string.IsNullOrEmpty(operatorIdentifier))
            {
                operatorLoginId  = await (
                    from lp in this.DbContext.Set<LoginProfile>()

                    join l in this.DbContext.Set<Login>()
                        on lp.LoginId equals l.Id

                    join p in this.DbContext.Set<Profile>()
                        on l.ElectronicSubjectId equals p.ElectronicSubjectId

                    where lp.ProfileId == profileLogin.ProfileId
                        && l.IsActive
                        && EF.Functions.Like(p.Identifier, operatorIdentifier)

                    select lp.LoginId)
                    .FirstOrDefaultAsync(ct);

                if (operatorLoginId == 0)
                {
                    throw new System.Exception("Invalid operator id");
                }
            }

            int? representedProfileId = null;
            if (!string.IsNullOrEmpty(representedIdentifier))
            {
                representedProfileId = await (
                    from p in this.DbContext.Set<Profile>()

                    where p.IsActivated
                        && EF.Functions.Like(p.Identifier, representedIdentifier)

                    select p.Id)
                    .FirstOrDefaultAsync(ct);

                if (representedProfileId == 0)
                {
                    throw new System.Exception("Invalid represented person id");
                }
            }

            return new(
                profileLogin.ProfileId,
                profileLogin.LoginId,
                operatorLoginId,
                representedProfileId);
        }
    }
}
