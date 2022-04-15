using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfilesCreateEditViewQueryRepository
    {
        Task<GetBlobVO> GetBlobAsync(
            int blobId,
            CancellationToken ct);
    }
}
