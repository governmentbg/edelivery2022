using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateViberDeliveryQueueMessageCommandHandler(
        IUnitOfWork UnitOfWork,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<CreateViberDeliveryQueueMessageCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateViberDeliveryQueueMessageCommand command,
            CancellationToken ct)
        {
            ViberDeliveryCheckQueueMessage viberDeliveryCheckQueueMessage =
                new(command.Feature, command.ViberId);

            await this.QueueMessagesService.PostMessageAsync(
                viberDeliveryCheckQueueMessage,
                command.DueDate,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
