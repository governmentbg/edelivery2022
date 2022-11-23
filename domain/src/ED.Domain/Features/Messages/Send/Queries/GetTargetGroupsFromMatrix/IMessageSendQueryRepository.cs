using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<GetTargetGroupsFromMatrixVO>> GetTargetGroupsFromMatrixAsync(
            int profileId,
            CancellationToken ct);
    }
}
