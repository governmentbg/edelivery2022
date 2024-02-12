using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateSmsViberDeliveryCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<SmsDelivery> SmsDeliveryAggregateRepository,
        IAggregateRepository<ViberDelivery> ViberDeliveryAggregateRepository)
        : IRequestHandler<CreateSmsViberDeliveryCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateSmsViberDeliveryCommand command,
            CancellationToken ct)
        {
            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            foreach (CreateSmsViberDeliveryCommandMessages message in command.Messages)
            {
                switch (message.Type)
                {
                    case DeliveryResultType.Sms:
                        SmsDelivery smsDelivery = new(
                            message.Status,
                            command.MsgId,
                            message.Charge,
                            command.Tag);

                        await this.SmsDeliveryAggregateRepository.AddAsync(
                            smsDelivery,
                            ct);
                        break;
                    case DeliveryResultType.Viber:
                        ViberDelivery viberDelivery = new(
                            message.Status,
                            command.MsgId,
                            message.Charge,
                            command.Tag);

                        await this.ViberDeliveryAggregateRepository.AddAsync(
                            viberDelivery,
                            ct);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Incorrect value for {nameof(CreateSmsViberDeliveryCommandMessages.Type)} parameter");
                }

                await this.UnitOfWork.SaveAsync(ct);
            }

            await transaction.CommitAsync(ct);

            return default;
        }
    }
}
