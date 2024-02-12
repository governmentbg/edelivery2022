using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateMessageTranslationCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<MessageTranslation> MessageTranslationAggregateRepository,
        ITranslationsCreateEditViewQueryRepository TranslationsCreateEditViewQueryRepository,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<CreateMessageTranslationCommand, Unit>
    {
        public async Task<Unit> Handle(
            CreateMessageTranslationCommand command,
            CancellationToken ct)
        {
            int?[] blobIds =
                (await this.TranslationsCreateEditViewQueryRepository.GetMessageBlobIdsAsync(
                    command.MessageId,
                    ct))
                    .Select(e => (int?)e)
                    .ToArray();

            int?[] sourceBlobIds = (new int?[] { null }).Concat(blobIds).ToArray();

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            MessageTranslation messageTranslation = new(
                command.MessageId,
                command.ProfileId,
                command.SourceLanguage,
                command.TargetLanguage,
                command.LoginId,
                sourceBlobIds);

            await this.MessageTranslationAggregateRepository.AddAsync(messageTranslation, ct);

            await this.UnitOfWork.SaveAsync(ct);

            TranslationQueueMessage translationQueueMessage = new(
                messageTranslation.MessageTranslationId);

            await this.QueueMessagesService.PostMessageAsync(
                translationQueueMessage,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return default;
        }
    }
}
