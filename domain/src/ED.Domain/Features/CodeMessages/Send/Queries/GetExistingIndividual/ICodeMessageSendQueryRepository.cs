using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageSendQueryRepository
    {
        Task<GetExistingIndividualVO?> GetExistingIndividualAsync(
            string identifier,
            CancellationToken ct);
    }
}
