using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static ED.Domain.ITicketsOpenQueryRepository;

namespace ED.Domain
{
    partial class TicketsOpenQueryRepository : ITicketsOpenQueryRepository
    {
        public async Task<GetTicketTypeAndDocumentIdentifierVO> GetTicketTypeAndDocumentIdentifierAsync(
            int messageId,
            CancellationToken ct)
        {
            GetTicketTypeAndDocumentIdentifierVO vo = await (
                from t in this.DbContext.Set<Ticket>()

                where t.MessageId == messageId

                select new GetTicketTypeAndDocumentIdentifierVO(
                    t.Type,
                    t.DocumentIdentifier))
                .SingleAsync(ct);

            return vo;
        }
    }
}
