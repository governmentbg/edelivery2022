using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class AdminRegistrationsEditQueryRepository : IAdminRegistrationsEditQueryRepository
    {
        public async Task<bool> HasExistingProfileAsync(
            string identifier,
            int targetGroupId,
            CancellationToken ct)
        {
            // TODO: rethink if passive profiles should be considered
            bool hasExistingLegalEntity = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                join rr in this.DbContext.Set<RegistrationRequest>()
                    on p.Id equals rr.RegisteredProfileId
                    into lj1
                from rr in lj1.DefaultIfEmpty()

                where EF.Functions.Like(p.Identifier, identifier)
                    && tgp.TargetGroupId == targetGroupId
                    && (p.IsActivated || rr.Status == RegistrationRequestStatus.New)

                select p.Id)
                .AnyAsync(ct);

            return hasExistingLegalEntity;
        }
    }
}
