using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<GetRegistrationRequestBlobVO> GetRegistrationRequestBlobAsync(
            int registrationRequestId,
            CancellationToken ct);
    }
}
