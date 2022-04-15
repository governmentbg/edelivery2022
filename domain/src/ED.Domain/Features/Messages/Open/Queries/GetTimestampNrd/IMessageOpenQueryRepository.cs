using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        Task<GetTimestampNrdVO> GetTimestampNrdAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
