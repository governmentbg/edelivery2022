using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<GetLegalEntityVO> GetLegalEntityAsync(
            int profileId,
            CancellationToken ct);
    }
}
