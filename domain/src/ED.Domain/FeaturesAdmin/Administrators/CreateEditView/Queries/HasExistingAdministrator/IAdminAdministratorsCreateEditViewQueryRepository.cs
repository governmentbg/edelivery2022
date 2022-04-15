using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminAdministratorsCreateEditViewQueryRepository
    {
        Task<bool> HasExistingAdministratorAsync(
            string userName,
            CancellationToken ct);
    }
}
