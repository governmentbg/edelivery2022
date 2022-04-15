using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        Task<GetPdfAsSenderVO> GetPdfAsSenderAsync(
            int messageId,
            CancellationToken ct);
    }
}
