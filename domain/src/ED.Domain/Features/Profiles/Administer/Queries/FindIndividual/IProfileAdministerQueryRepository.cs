using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<FindIndividualVO?> FindIndividualAsync(
            string firstName,
            string lastName,
            string identifier,
            CancellationToken ct);
    }
}
