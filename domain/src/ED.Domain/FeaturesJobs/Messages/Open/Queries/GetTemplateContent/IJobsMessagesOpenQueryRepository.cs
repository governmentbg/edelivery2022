using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IJobsMessagesOpenQueryRepository
    {
        Task<GetTemplateContentVO> GetTemplateContentAsync(
            int templateId,
            CancellationToken ct);
    }
}
