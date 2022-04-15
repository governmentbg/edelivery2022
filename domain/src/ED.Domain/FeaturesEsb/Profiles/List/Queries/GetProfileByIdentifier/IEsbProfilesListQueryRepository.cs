using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesListQueryRepository
    {
        Task<GetProfileByIdentifierVO?> GetProfileByIdentifierAsync(
            string identifier,
            CancellationToken ct);
    }
}
