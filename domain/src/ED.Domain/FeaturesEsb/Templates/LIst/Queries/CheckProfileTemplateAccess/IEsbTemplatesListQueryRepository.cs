using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTemplatesListQueryRepository
    {
        Task<bool> CheckProfileTemplateAccessAsync(
            int profileId,
            int templateId,
            CancellationToken ct);
    }
}
