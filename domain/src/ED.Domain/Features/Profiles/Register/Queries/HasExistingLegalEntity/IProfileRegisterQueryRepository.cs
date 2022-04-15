using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        Task<bool> HasExistingLegalEntityAsync(
            string identifier,
            CancellationToken ct);
    }
}
