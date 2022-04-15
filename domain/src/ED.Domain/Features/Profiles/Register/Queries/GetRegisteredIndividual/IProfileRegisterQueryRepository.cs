using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileRegisterQueryRepository
    {
        Task<GetRegisteredIndividualVO?> GetRegisteredIndividualAsync(
            string identifier,
            CancellationToken ct);
    }
}
