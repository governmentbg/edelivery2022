using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RemoveProfileBlobCommandHandler(
        IUnitOfWork UnitOfWork,
        IProfileBlobAccessKeyAggregateRepository ProfileBlobAccessKeyAggregateRepository)
        : IRequestHandler<RemoveProfileBlobCommand>
    {
        public async Task<Unit> Handle(
            RemoveProfileBlobCommand command,
            CancellationToken ct)
        {
            ProfileBlobAccessKey? profileBlobAccessKey =
                await this.ProfileBlobAccessKeyAggregateRepository.FindAsync(
                command.ProfileId,
                command.BlobId,
                ProfileBlobAccessKeyType.Storage,
                ct);

            if (profileBlobAccessKey != null)
            {
                this.ProfileBlobAccessKeyAggregateRepository.Remove(profileBlobAccessKey);
            }

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
