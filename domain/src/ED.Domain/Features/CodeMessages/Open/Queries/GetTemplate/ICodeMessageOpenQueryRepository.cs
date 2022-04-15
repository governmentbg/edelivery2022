using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetTemplateVO> GetTemplateAsync(
            int templateId,
            CancellationToken ct);
    }
}
