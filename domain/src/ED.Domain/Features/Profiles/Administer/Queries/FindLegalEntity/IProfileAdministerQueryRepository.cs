using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<FindLegalEntityVO?> FindLegalEntityAsync(
            string identifier,
            CancellationToken ct);
    }
}
