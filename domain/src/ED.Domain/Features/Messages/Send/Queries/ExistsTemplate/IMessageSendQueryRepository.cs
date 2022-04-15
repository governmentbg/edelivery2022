using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<bool> ExistsTemplateAsync(
            int templateId,
            CancellationToken ct);
    }
}
