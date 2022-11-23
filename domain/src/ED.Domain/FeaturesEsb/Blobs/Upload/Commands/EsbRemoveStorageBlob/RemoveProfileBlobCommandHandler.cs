using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record EsbRemoveStorageBlobCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository)
        : IRequestHandler<EsbRemoveStorageBlobCommand>
    {
        public async Task<Unit> Handle(
            EsbRemoveStorageBlobCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            profile.RemoveBlob(command.BlobId, ProfileBlobAccessKeyType.Storage);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
