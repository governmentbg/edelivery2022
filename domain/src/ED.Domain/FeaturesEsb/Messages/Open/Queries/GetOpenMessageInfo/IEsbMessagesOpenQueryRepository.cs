using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        Task<GetOpenMessageInfoVO> GetOpenMessageInfoAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
