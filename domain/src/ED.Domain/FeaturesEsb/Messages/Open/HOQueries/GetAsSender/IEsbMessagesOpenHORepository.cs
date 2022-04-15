using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenHORepository
    {
        Task<GetAsSenderVO> GetAsSenderAsync(
            int messageId,
            CancellationToken ct);
    }
}
