using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateTicketDeliveryCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<TicketDelivery> TicketDeliveryAggregateRepository)
        : IRequestHandler<CreateTicketDeliveryCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateTicketDeliveryCommand command,
            CancellationToken ct)
        {
            TicketDelivery ticketDelivery = new(command.MessageId, command.Status);

            await this.TicketDeliveryAggregateRepository.AddAsync(
                ticketDelivery,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
