using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenHORepository
    {
        Task<GetAsRecipientVO> GetAsRecipientAsync(
            int messageId,
            CancellationToken ct);
    }
}
