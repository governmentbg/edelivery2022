using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record JobsUpdateMessageTranslationRequestCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<MessageTranslation> MessageTranslationAggregateRepository)
        : IRequestHandler<JobsUpdateMessageTranslationRequestCommand, Unit>
    {
        public async Task<Unit> Handle(
            JobsUpdateMessageTranslationRequestCommand command,
            CancellationToken ct)
        {
            MessageTranslation messageTranslation =
                await this.MessageTranslationAggregateRepository.FindAsync(
                    command.MessageTranslationId,
                    ct);

            messageTranslation.UpdateRequest(
                command.SourceBlobId,
                command.RequestId,
                command.Status,
                command.ErrorMessage);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
