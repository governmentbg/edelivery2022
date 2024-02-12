using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        Task<bool> CheckAllLoginsExistAsync(
            string[] identifiers,
            CancellationToken ct);
    }
}
