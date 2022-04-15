using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITemplateListQueryRepository
    {
        Task<GetContentVO> GetContentAsync(
            int templateId,
            CancellationToken ct);
    }
}
