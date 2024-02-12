using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    partial class EsbProfilesRegisterQueryRepository : IEsbProfilesRegisterQueryRepository
    {
        public async Task<bool> CheckTargetGroupIdAsync(
            int targetGroupId,
            CancellationToken ct)
        {
            bool isValid = await (
                from tg in this.DbContext.Set<TargetGroup>()

                where tg.TargetGroupId == targetGroupId

                select tg.TargetGroupId)
                .AnyAsync(ct);

            return isValid;
        }
    }
}
