using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRecipientGroupsListQueryRepository
    {
        Task<TableResultVO<GetAllVO>> GetAllAsync(
            int offset,
            int limit,
            CancellationToken ct);
    }
}
