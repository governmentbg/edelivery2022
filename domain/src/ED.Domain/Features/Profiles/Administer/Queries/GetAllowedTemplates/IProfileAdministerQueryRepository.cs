using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<TableResultVO<GetAllowedTemplatesVO>> GetAllowedTemplatesAsync(
            int profileId,
            CancellationToken ct);
    }
}
