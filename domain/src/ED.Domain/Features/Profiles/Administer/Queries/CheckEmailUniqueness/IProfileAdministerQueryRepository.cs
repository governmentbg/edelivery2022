using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        Task<bool> CheckEmailUniquenessAsync(
            string identifier,
            string email,
            CancellationToken ct);
    }
}
