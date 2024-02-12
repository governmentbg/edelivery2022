using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminProfileListQueryRepository
    {
        Task<TableResultVO<GetProfilesVO>> GetProfilesAsync(
            int adminUserId,
            string? identifier,
            string? nameEmailPhone,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
