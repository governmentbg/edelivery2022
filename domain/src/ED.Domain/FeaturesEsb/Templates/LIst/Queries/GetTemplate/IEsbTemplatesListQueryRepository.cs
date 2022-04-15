using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTemplatesListQueryRepository
    {
        Task<GetTemplateVO> GetTemplateAsync(
            int templateId,
            CancellationToken ct);
    }
}
