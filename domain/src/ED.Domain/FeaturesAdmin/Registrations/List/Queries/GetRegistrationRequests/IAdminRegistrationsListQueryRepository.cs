using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsListQueryRepository
    {
        Task<TableResultVO<GetRegistrationRequestsVO>> GetRegistrationRequestsAsync(
            int adminUserId,
            int? status,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
