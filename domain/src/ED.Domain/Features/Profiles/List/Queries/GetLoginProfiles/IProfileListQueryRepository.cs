using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        Task<GetLoginProfilesVO[]> GetLoginProfilesAsync(
            int loginId,
            CancellationToken ct);
    }
}
