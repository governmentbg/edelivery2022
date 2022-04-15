using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbTemplatesListQueryRepository
    {
        Task<GetTemplatesVO[]> GetTemplatesAsync(
            int profileId,
            CancellationToken ct);
    }
}
