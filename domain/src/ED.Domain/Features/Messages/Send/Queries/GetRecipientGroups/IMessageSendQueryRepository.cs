using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<GetRecipientGroupsVO>> GetRecipientGroupsAsync(
            string? term,
            int profileId,
            int templateId,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
