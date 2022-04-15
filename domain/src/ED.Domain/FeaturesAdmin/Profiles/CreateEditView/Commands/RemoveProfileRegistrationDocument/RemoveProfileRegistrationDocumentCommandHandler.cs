using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RemoveProfileRegistrationDocumentCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<RemoveProfileRegistrationDocumentCommand, Unit>
    {
        public async Task<Unit> Handle(
            RemoveProfileRegistrationDocumentCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            profile.RemoveBlob(command.BlobId, ProfileBlobAccessKeyType.Registration);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
