using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesAuthenticateQueryRepository
    {
        Task<GetEsbUserVO?> GetEsbUserAsync(
            string oId,
            string clientId,
            string? operatorIdentifier,
            string? representedIdentifier,
            CancellationToken ct);
    }
}
