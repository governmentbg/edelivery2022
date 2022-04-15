using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetSenderProfileVO> GetSenderProfileAsync(
            int messageId,
            CancellationToken ct);
    }
}
