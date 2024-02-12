using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<GetProfileVO?> GetProfileAsync(
            int profileId,
            CancellationToken ct);
    }
}
