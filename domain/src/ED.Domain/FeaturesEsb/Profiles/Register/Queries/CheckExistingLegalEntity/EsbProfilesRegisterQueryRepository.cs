using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<bool> CheckExistingLegalEntityAsync(
            string identifier,
            int targetGroupId,
            CancellationToken ct)
        {
            bool existingIndividual = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && p.IsActivated
                    && tgp.TargetGroupId == targetGroupId

                select p.Id)
                .AnyAsync(ct);

            return existingIndividual;
        }
    }
}
