using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        Task<GetAsSenderVO> GetAsSenderAsync(
            int messageId,
            CancellationToken ct);
    }
}
