using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        Task<int?> GetMessageTemplateAsync(
            int messageId,
            CancellationToken ct);
    }
}
