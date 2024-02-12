using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record ServeTicketCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Ticket> TicketAggregateRepository,
        IEsbTicketsSendQueryRepository EsbTicketsSendQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<ServeTicketCommand, Unit>
    {
        public async Task<Unit> Handle(
            ServeTicketCommand command,
            CancellationToken ct)
        {
            Ticket ticket =
                await this.TicketAggregateRepository.FindAsync(command.TicketId, ct);

            ticket.ExternalServe(command.ServeDate, command.ActionLoginId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
