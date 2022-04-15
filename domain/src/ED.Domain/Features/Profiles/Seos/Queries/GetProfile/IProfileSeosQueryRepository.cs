using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IProfileSeosQueryRepository
    {
        Task<GetProfileVO> GetProfileAsync(
            int profileId,
            CancellationToken ct);
    }
}
