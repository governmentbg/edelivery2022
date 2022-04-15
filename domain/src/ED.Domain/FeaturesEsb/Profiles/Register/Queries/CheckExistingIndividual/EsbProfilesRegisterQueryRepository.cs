using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<bool> CheckExistingIndividualAsync(
            string identifier,
            CancellationToken ct)
        {
            bool existingIndividual = await (
                from p in this.DbContext.Set<Profile>()

                join tgp in this.DbContext.Set<TargetGroupProfile>()
                    on p.Id equals tgp.ProfileId

                where EF.Functions.Like(p.Identifier, identifier)
                    && (p.IsActivated || p.IsPassive)
                    && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

                select p.Id)
                .AnyAsync(ct);

            return existingIndividual;
        }
    }
}
