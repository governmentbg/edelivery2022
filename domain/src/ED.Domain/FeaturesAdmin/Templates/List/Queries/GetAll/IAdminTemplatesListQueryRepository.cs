using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTemplatesListQueryRepository
    {
        Task<TableResultVO<GetAllVO>> GetAllAsync(
            int offset,
            int limit,
            CancellationToken ct);
    }
}
