using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<FindRecipientProfilesVO>> FindRecipientProfilesAsync(
            string query,
            int? targetGroupId,
            int templateId,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
