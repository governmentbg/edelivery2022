using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<FindProfilesVO>> FindProfilesAsync(
            string query,
            int? targetGroupId,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
