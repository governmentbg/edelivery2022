using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record JobsCloseMessageTranslationRequestsCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<MessageTranslation> MessageTranslationAggregateRepository)
        : IRequestHandler<JobsCloseMessageTranslationRequestsCommand, Unit>
    {
        public async Task<Unit> Handle(
            JobsCloseMessageTranslationRequestsCommand command,
            CancellationToken ct)
        {
            MessageTranslation messageTranslation =
                await this.MessageTranslationAggregateRepository.FindAsync(
                    command.MessageTranslationId,
                    ct);

            messageTranslation.CloseRequests();

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
