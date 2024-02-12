using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateEmailDeliveryCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<EmailDelivery> EmailDeliveryAggregateRepository)
        : IRequestHandler<CreateEmailDeliveryCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateEmailDeliveryCommand command,
            CancellationToken ct)
        {
            EmailDelivery emailDelivery = new(command.Status, command.Tag);

            await this.EmailDeliveryAggregateRepository.AddAsync(
                emailDelivery,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
