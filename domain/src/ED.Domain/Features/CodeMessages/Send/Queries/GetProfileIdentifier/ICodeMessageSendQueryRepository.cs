using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageSendQueryRepository
    {
        Task<string> GetProfileIdentifierAsync(
            int profileId,
            CancellationToken ct);
    }
}
