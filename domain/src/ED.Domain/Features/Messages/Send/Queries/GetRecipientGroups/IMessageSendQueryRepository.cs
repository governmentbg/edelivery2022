using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<GetRecipientGroupsVO>> GetRecipientGroupsAsync(
            int profileId,
            CancellationToken ct);
    }
}
