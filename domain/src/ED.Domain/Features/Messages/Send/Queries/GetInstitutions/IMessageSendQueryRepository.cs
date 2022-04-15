using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface IMessageSendQueryRepository
    {
        Task<TableResultVO<GetInstitutionsVO>> GetInstitutionsAsync(
            string term,
            int offset,
            int limit,
            CancellationToken ct);
    }
}
