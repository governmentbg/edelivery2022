using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateSmsDeliveryQueueMessageCommandHandler(
        IUnitOfWork UnitOfWork,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<CreateSmsDeliveryQueueMessageCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateSmsDeliveryQueueMessageCommand command,
            CancellationToken ct)
        {
            SmsDeliveryCheckQueueMessage smsDeliveryCheckQueueMessage =
                new(command.Feature, command.SmsId);

            await this.QueueMessagesService.PostMessageAsync(
                smsDeliveryCheckQueueMessage,
                command.DueDate,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
