using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record EsbRemoveStorageBlobCommandHandler(
        IUnitOfWork UnitOfWork,
        IProfileBlobAccessKeyAggregateRepository ProfileBlobAccessKeyAggregateRepository)
        : IRequestHandler<EsbRemoveStorageBlobCommand>
    {
        public async Task<Unit> Handle(
            EsbRemoveStorageBlobCommand command,
            CancellationToken ct)
        {
            ProfileBlobAccessKey? profileBlobAccessKey =
                await this.ProfileBlobAccessKeyAggregateRepository.FindAsync(
                command.ProfileId,
                command.BlobId,
                ProfileBlobAccessKeyType.Registration,
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
