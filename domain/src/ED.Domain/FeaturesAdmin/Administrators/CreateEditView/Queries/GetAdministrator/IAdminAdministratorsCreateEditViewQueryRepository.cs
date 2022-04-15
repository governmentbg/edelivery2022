using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminAdministratorsCreateEditViewQueryRepository
    {
        Task<GetAdministratorVO> GetAdministratorAsync(
            int id,
            CancellationToken ct);
    }
}
