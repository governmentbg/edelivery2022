using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminProfilesCreateEditViewQueryRepository : IAdminProfilesCreateEditViewQueryRepository
    {
        public async Task<bool> HasActiveOrPendingProfileAsync(
            int profileId,
            string identifier,
            int targetGroupId,
            CancellationToken ct)
        {
            bool hasActiveOrPendingProfile = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join rr in this.DbContext.Set<RegistrationRequest>()
                    on p.Id equals rr.RegisteredProfileId
                    into lj1
                from rr in lj1.DefaultIfEmpty()

                where (p.Id != profileId
                    && p.Identifier == identifier
                    && tgp.TargetGroupId == targetGroupId
                    && p.IsActivated)
                    || (p.Id == profileId
                        && rr != null
                        && rr.Status != RegistrationRequestStatus.Confirmed) // profile has not been confirmed for registration

                select p)
                .AnyAsync(ct);

            return hasActiveOrPendingProfile;
        }
    }
}
