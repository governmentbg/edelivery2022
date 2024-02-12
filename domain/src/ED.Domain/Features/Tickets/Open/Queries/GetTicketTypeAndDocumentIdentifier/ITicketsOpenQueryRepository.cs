using System.Threading;
using System.Threading.Tasks;

namespace ED.Domain
{
    public partial interface ITicketsOpenQueryRepository
    {
        Task<GetTicketTypeAndDocumentIdentifierVO> GetTicketTypeAndDocumentIdentifierAsync(
            int messageId,
            CancellationToken ct);
    }
}
