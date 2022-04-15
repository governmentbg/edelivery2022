using System.Threading;
using System.Threading.Tasks;
using ED.Keystore;
using Google.Protobuf;
using MediatR;
using static ED.Domain.IAdminProfilesCreateEditViewQueryRepository;
using static ED.Keystore.Keystore;

namespace ED.Domain
{
    internal record AddProfileRegistrationDocumentCommandHandler(
        KeystoreClient KeystoreClient,
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IProfilesService ProfilesService,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<AddProfileRegistrationDocumentCommand, Unit>
    {
        public async Task<Unit> Handle(
            AddProfileRegistrationDocumentCommand command,
            CancellationToken ct)
        {
            GetBlobVO blob =
                await this.AdminProfilesCreateEditViewQueryRepository
                    .GetBlobAsync(command.BlobId, ct);

            DecryptWithRsaKeyResponse decryptedKeyResp =
               await this.KeystoreClient.DecryptWithRsaKeyAsync(
                   request: new DecryptWithRsaKeyRequest
                   {
                       Key = new RsaKey
                       {
                           Provider = blob.SystemProfileKeyProvider,
                           KeyName = blob.SystemProfileKeyKeyName,
                           OaepPadding = blob.SystemProfileKeyOaepPadding,
                       },
                       EncryptedData = ByteString.CopyFrom(blob.EncryptedKey)
                   },
                   cancellationToken: ct);

            IProfilesService.ProfileKeyVO profileKey =
                await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                    command.ProfileId,
                    ct);

            EncryptWithRsaKeyResponse encryptedKeyResp =
                await this.KeystoreClient.EncryptWithRsaKeyAsync(
                    request: new EncryptWithRsaKeyRequest
                    {
                        Key = new RsaKey
                        {
                            Provider = profileKey.Provider,
                            KeyName = profileKey.KeyName,
                            OaepPadding = profileKey.OaepPadding,
                        },
                        Plaintext = decryptedKeyResp.Plaintext
                    },
                    cancellationToken: ct);

            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            profile.AddBlob(
                command.BlobId,
                profileKey.ProfileKeyId,
                null,
                command.AdminUserId,
                encryptedKeyResp.EncryptedData.ToByteArray(),
                ProfileBlobAccessKeyType.Registration);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
