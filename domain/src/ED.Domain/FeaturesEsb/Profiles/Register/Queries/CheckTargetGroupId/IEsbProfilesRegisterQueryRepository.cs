using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        Task<bool> CheckTargetGroupIdAsync(
            int targetGroupId,
            CancellationToken ct);
    }
}
