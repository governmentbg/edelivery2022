using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IAdminRegistrationsEditQueryRepository
    {
        Task<string> GetLoginNameAsync(
            int loginId,
            CancellationToken ct);
    }
}
