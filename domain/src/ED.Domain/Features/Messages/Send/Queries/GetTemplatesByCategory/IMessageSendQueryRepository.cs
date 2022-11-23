using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<GetTemplatesByCategoryVO>> GetTemplatesByCategoryAsync(
            int profileId,
            int loginId,
            string? category,
            CancellationToken ct);
    }
}
