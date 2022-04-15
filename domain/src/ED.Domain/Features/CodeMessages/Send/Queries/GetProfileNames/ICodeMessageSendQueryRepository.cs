using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageSendQueryRepository
    {
        Task<GetProfileNamesVO[]> GetProfileNamesAsync(
            int[] profileIds,
            CancellationToken ct);
    }
}
