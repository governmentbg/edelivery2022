using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<ParseRegistrationDocumentVO> ParseRegistrationDocumentAsync(
            int blobId,
            CancellationToken ct);
    }
}
