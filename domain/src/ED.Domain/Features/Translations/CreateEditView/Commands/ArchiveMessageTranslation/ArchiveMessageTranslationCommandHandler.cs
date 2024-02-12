using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ArchiveMessageTranslationCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<MessageTranslation> MessageTranslationAggregateRepository,
        IProfileBlobAccessKeyAggregateRepository ProfileBlobAccessKeyAggregateRepository,
        ITranslationsCreateEditViewQueryRepository TranslationsCreateEditViewQueryRepository,
        IQueueMessagesService QueueMessagesService)
        : IRequestHandler<ArchiveMessageTranslationCommand, Unit>
    {
        public async Task<Unit> Handle(
            ArchiveMessageTranslationCommand command,
            CancellationToken ct)
        {
            MessageTranslation messageTranslation =
                await this.MessageTranslationAggregateRepository.FindAsync(
                    command.MessageTranslationId,
                    ct);

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            foreach (MessageTranslationRequest request in messageTranslation.Requests.Where(e => e.TargetBlobId != null))
            {
                ProfileBlobAccessKey? profileBlobAccessKey =
                    await this.ProfileBlobAccessKeyAggregateRepository.FindAsync(
                    messageTranslation.ProfileId,
                    request.TargetBlobId!.Value,
                    ProfileBlobAccessKeyType.Translation,
                    ct);

                if (profileBlobAccessKey != null)
                {
                    this.ProfileBlobAccessKeyAggregateRepository.Remove(profileBlobAccessKey);
                }
            }

            await this.UnitOfWork.SaveAsync(ct);

            messageTranslation.Archive(command.LoginId);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return default;
        }
    }
}
