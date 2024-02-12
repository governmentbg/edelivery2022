using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IEsbProfilesRegisterQueryRepository
    {
        Task<bool> CheckExistingLegalEntityAsync(
            string identifier,
            int targetGroupId,
            CancellationToken ct);
    }
}
