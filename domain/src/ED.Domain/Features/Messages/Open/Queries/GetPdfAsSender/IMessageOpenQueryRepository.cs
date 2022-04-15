using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetPdfAsSenderVO> GetPdfAsSenderAsync(
            int messageId,
            CancellationToken ct);
    }
}
