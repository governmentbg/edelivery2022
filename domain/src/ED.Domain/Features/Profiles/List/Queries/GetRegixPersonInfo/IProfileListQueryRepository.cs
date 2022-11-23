using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileListQueryRepository
    {
        Task<GetRegixPersonInfoVO?> GetRegixPersonInfoAsync(
            string identifier,
            CancellationToken ct);
    }
}
