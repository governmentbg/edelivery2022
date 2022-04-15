using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        Task<GetVO> GetAsync(int templateId, CancellationToken ct);
    }
}
