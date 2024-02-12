using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbTicketsSendQueryRepository : IEsbTicketsSendQueryRepository
    {
        public async Task<int?> GetIndividualProfileIdAsync(
            string identifier,
            CancellationToken ct)
        {
            int? profileId = await (
              from p in this.DbContext.Set<Profile>()

              join tgp in this.DbContext.Set<TargetGroupProfile>()
                  on p.Id equals tgp.ProfileId

              where EF.Functions.Like(p.Identifier, identifier)
                  && tgp.TargetGroupId == TargetGroup.IndividualTargetGroupId

              orderby p.IsActivated descending

              select p.Id)
              .Cast<int?>()
              .FirstOrDefaultAsync(ct);

            return profileId;
        }
    }
}
