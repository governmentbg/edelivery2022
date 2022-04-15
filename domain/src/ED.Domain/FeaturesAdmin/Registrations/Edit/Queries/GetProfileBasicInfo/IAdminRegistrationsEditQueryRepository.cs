using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<GetProfileBasicInfoVO> GetProfileBasicInfoAsync(
            int requestRegistrationId,
            CancellationToken ct);
    }
}
