using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminTemplatesCreateEditViewQueryRepository
    {
        Task<GetPermissionsVO> GetPermissionsAsync(
            int templateId,
            CancellationToken ct);
    }
}
