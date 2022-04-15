using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetSenderProfileVO> GetSenderProfileAsync(
            string accessCode,
            CancellationToken ct);
    }
}
