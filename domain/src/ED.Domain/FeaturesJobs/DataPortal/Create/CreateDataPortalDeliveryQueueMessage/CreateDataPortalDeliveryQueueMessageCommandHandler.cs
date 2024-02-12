using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateDataPortalDeliveryQueueMessageCommandHandler(
        IUnitOfWork UnitOfWork,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<CreateDataPortalDeliveryQueueMessageCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateDataPortalDeliveryQueueMessageCommand command,
            CancellationToken ct)
        {
            DataPortalQueueMessage dataPortalQueueMessage = new(
                QueueMessageFeatures.DataPortal,
                command.DatasetUri,
                command.Type);

            await this.QueueMessagesService.PostMessageAsync(
                dataPortalQueueMessage,
                command.DueDate,
                QueueMessageFeatures.DataPortal,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
