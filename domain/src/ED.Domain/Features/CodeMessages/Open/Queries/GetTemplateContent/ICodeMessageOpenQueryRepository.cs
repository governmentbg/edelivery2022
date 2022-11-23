using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<string> GetTemplateContentAsync(
            int templateId,
            CancellationToken ct);
    }
}
