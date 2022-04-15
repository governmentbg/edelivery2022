using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<bool> CheckProfileOnBehalfOfAccessAsync(
            int profileId,
            CancellationToken ct);
    }
}
