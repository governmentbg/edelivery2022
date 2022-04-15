using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageListQueryRepository
    {
        Task<GetNewMessagesCountVO[]> GetNewMessagesCountAsync(
            int loginId,
            CancellationToken ct);
    }
}
