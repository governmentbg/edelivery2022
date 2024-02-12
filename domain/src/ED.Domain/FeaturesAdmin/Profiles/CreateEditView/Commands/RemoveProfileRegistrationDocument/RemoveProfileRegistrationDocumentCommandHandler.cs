using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RemoveProfileRegistrationDocumentCommandHandler(
        IUnitOfWork UnitOfWork,
        IProfileBlobAccessKeyAggregateRepository ProfileBlobAccessKeyAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<RemoveProfileRegistrationDocumentCommand, Unit>
    {
        public async Task<Unit> Handle(
            RemoveProfileRegistrationDocumentCommand command,
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
