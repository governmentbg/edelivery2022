using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<GetRegistrationRequestVO> GetRegistrationRequestAsync(
            int adminUserId,
            int registrationRequestId,
            CancellationToken ct);
    }
}
