using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RemoveProfileBlobCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository)
        : IRequestHandler<RemoveProfileBlobCommand>
    {
        public async Task<Unit> Handle(
            RemoveProfileBlobCommand command,
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
