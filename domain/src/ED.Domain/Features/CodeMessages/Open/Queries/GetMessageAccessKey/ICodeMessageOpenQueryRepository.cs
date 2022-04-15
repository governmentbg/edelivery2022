using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ICodeMessageOpenQueryRepository
    {
        Task<GetMessageAccessKeyVO> GetMessageAccessKeyAsync(
            int messageId,
            int profileId,
            CancellationToken ct);
    }
}
