using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<GetRegistrationSystemProfileBlobVO> GetRegistrationSystemProfileBlobAsync(
            int blobId,
            CancellationToken ct);
    }
}
