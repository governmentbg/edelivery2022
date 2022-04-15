using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        Task<ParseRegistrationDocumentVO> ParseRegistrationDocumentAsync(
            int blobId,
            CancellationToken ct);
    }
}
