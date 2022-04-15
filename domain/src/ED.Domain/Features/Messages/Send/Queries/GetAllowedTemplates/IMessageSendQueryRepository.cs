using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<GetAllowedTemplatesVO>> GetAllowedTemplatesAsync(
            int profileId,
            int loginId,
            CancellationToken ct);
    }
}
