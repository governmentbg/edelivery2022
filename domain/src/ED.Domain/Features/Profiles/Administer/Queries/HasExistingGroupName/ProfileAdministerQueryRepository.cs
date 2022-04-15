using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class ProfileAdministerQueryRepository : IProfileAdministerQueryRepository
    {
        public async Task<bool> HasExistingGroupName(
            string groupName,
            int profileId,
            CancellationToken ct)
        {
            bool hasExistingGroupName = await (
                from rg in this.DbContext.Set<RecipientGroup>()

                where EF.Functions.Like(rg.Name, groupName)
                    && rg.ProfileId == profileId
                    && !rg.ArchiveDate.HasValue

                select rg.RecipientGroupId)
                .AnyAsync(ct);

            return hasExistingGroupName;
        }
    }
}
