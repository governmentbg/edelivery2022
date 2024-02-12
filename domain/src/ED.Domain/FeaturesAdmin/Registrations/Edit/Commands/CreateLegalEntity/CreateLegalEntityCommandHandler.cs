using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;
using static ED.Keystore.Keystore;

namespace ED.Domain
{
    internal record CreateLegalEntityCommandHandler(
        IUnitOfWork UnitOfWork,
        KeystoreClient KeystoreClient,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IProfileBlobAccessKeyAggregateRepository ProfileBlobAccessKeyAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<RegistrationRequest> RegistrationRequestAggregateRepository,
        IAdminRegistrationsEditQueryRepository AdminRegistrationsEditQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<CreateLegalEntityCommand, CreateLegalEntityCommandResult>
    {
        private const string NotificationEvent = "OnAdminRegistration";

        private const string RegisterLegalEntityActionDetails = "Register legal entity from admin panel";

        public async Task<CreateLegalEntityCommandResult> Handle(
            CreateLegalEntityCommand command,
            CancellationToken ct)
        {
            bool hasExistingProfile =
                await this.AdminRegistrationsEditQueryRepository.HasExistingProfileAsync(
                    command.Identifier,
                    command.TargetGroupId,
                    ct);

            if (hasExistingProfile)
            {
                return new CreateLegalEntityCommandResult(
                    null,
                    false,
                    "There is an active or pending for registration profile with the same identifier.");
            }

            // create the key upfront to minimize the transaction lifetime
            Keystore.CreateRsaKeyResponse newRsaKey =
                await this.KeystoreClient.CreateRsaKeyAsync(
                    request: new Empty(),
                    cancellationToken: ct);

            DateTime issuedAt = DateTime.Now;
            DateTime expiresAt =
                issuedAt.Add(this.DomainOptionsAccessor.Value.ProfileKeyExpiration);

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            Profile profile = new(
                command.Name,
                command.Identifier,
                command.Phone,
                command.Email,
                command.Residence,
                command.AdminUserId,
                newRsaKey.Key.Provider,
                newRsaKey.Key.KeyName,
                newRsaKey.Key.OaepPadding,
                issuedAt,
                expiresAt);

            await this.ProfileAggregateRepository.AddAsync(profile, ct);

            await this.UnitOfWork.SaveAsync(ct);

            // TODO: is part of profile aggregate
            await this.TargetGroupProfileAggregateRepository.AddAsync(
                new TargetGroupProfile(command.TargetGroupId, profile.Id), ct);

            await this.UnitOfWork.SaveAsync(ct);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
                profile.Id,
                ProfileHistoryAction.ProfileActivated,
                command.AdminUserId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.CreateProfile,
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    RegisterLegalEntityActionDetails),
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            if (command.BlobId.HasValue)
            {
                await this.RegistrationRequestAggregateRepository.AddAsync(
                    new RegistrationRequest(
                        profile.Id,
                        command.Email,
                        command.Phone,
                        command.BlobId.Value,
                        command.AdminUserId,
                        true),
                    ct);

                await this.UnitOfWork.SaveAsync(ct);

                var pk = profile.Keys.Single();
                ProfileBlobAccessKeyDO blobAccessKey =
                    await this.CreateBlobAccessKeyForDocument(
                       command.BlobId.Value,
                       pk.ProfileKeyId,
                       pk.Provider,
                       pk.KeyName,
                       pk.OaepPadding,
                       ct);

                ProfileBlobAccessKey profileBlobAccessKey = new(
                    profile.Id,
                    blobAccessKey.BlobId,
                    blobAccessKey.ProfileKeyId,
                    Login.SystemLoginId,
                    command.AdminUserId,
                    blobAccessKey.EncryptedKey,
                    ProfileBlobAccessKeyType.Registration);

                await this.ProfileBlobAccessKeyAggregateRepository.AddAsync(
                    profileBlobAccessKey,
                    ct);

                await this.UnitOfWork.SaveAsync(ct);
            }

            EmailQueueMessage emailQueueMessage = this.GetNotification(
              command.Email,
              profile.Id,
              profile.ElectronicSubjectName);

            await this.QueueMessagesService.PostMessageAsync(
                emailQueueMessage,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return new CreateLegalEntityCommandResult(
                profile.Id,
                true,
                string.Empty);
        }

        private record ProfileBlobAccessKeyDO(
            int BlobId,
            int ProfileKeyId,
            byte[] EncryptedKey);

        private async Task<ProfileBlobAccessKeyDO> CreateBlobAccessKeyForDocument(
            int blobId,
            int profileKeyId,
            string provider,
            string keyName,
            string oaepPadding,
            CancellationToken ct)
        {
            IAdminRegistrationsEditQueryRepository.GetRegistrationSystemProfileBlobVO blob =
                await this.AdminRegistrationsEditQueryRepository
                    .GetRegistrationSystemProfileBlobAsync(
                        blobId,
                        ct);

            Keystore.DecryptWithRsaKeyResponse decryptedKeyResp =
               await this.KeystoreClient.DecryptWithRsaKeyAsync(
                   request: new Keystore.DecryptWithRsaKeyRequest
                   {
                       Key = new Keystore.RsaKey
                       {
                           Provider = blob.SystemProfileKeyProvider,
                           KeyName = blob.SystemProfileKeyKeyName,
                           OaepPadding = blob.SystemProfileKeyOaepPadding,
                       },
                       EncryptedData = ByteString.CopyFrom(blob.EncryptedKey)
                   },
                   cancellationToken: ct);

            Keystore.EncryptWithRsaKeyResponse encryptedKeyResp =
                await this.KeystoreClient.EncryptWithRsaKeyAsync(
                    request: new Keystore.EncryptWithRsaKeyRequest
                    {
                        Key = new Keystore.RsaKey
                        {
                            Provider = provider,
                            KeyName = keyName,
                            OaepPadding = oaepPadding,
                        },
                        Plaintext = decryptedKeyResp.Plaintext
                    },
                    cancellationToken: ct);

            return new ProfileBlobAccessKeyDO(
                blob.BlobId,
                profileKeyId,
                encryptedKeyResp.EncryptedData.ToByteArray());
        }

        private EmailQueueMessage GetNotification(
            string email,
            int profileId,
            string profileName)
        {
            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.RegistrationProfileEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.RegistrationProfileEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.RegistrationProfileEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.RegistrationProfileEmailBody)}");

            string webPortalUrl =
              this.DomainOptionsAccessor.Value.WebPortalUrl
              ?? throw new Exception(
                  $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

            return new EmailQueueMessage(
                QueueMessageFeatures.Register,
                email,
                string.Format(emailSubect, profileName),
                string.Format(emailBody, profileName, webPortalUrl),
                false,
                new
                {
                    Event = NotificationEvent,
                    ProfileId = profileId
                });
        }
    }
}
