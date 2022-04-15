using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        Task<GetAsSenderVO> GetAsSenderAsync(
            int messageId,
            CancellationToken ct);
    }
}
