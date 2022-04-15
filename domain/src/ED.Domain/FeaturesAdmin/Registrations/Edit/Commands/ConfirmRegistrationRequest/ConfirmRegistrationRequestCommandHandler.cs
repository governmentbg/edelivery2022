using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record ConfirmRegistrationRequestCommandHandler(
        IUnitOfWork UnitOfWork,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IProfilesService ProfilesService,
        IAggregateRepository<RegistrationRequest> RegistrationRequestAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminRegistrationsEditQueryRepository AdminRegistrationsEditQueryRepository,
        IQueueMessagesService QueueMessagesService,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<ConfirmRegistrationRequestCommand, ConfirmRegistrationRequestCommandResult>
    {
        private const string NotificationEvent = "OnRegistrationConfirmed";

        public async Task<ConfirmRegistrationRequestCommandResult> Handle(
            ConfirmRegistrationRequestCommand command,
            CancellationToken ct)
        {
            IAdminRegistrationsEditQueryRepository.GetProfileBasicInfoVO info =
                await this.AdminRegistrationsEditQueryRepository
                    .GetProfileBasicInfoAsync(
                        command.RegistrationRequestId,
                        ct);

            bool hasActiveProfile =
                await this.AdminRegistrationsEditQueryRepository
                    .HasActiveProfileAsync(
                        info.ProfileId,
                        info.Identifier,
                        info.TargetGroupId,
                        ct);

            if (hasActiveProfile)
            {
                return new(
                    false,
                    "There is an active profile with the same identifier.");
            }

            await using ITransaction transaction =
               await this.UnitOfWork.BeginTransactionAsync(ct);

            RegistrationRequest registrationRequest =
                await this.RegistrationRequestAggregateRepository.FindAsync(
                    command.RegistrationRequestId,
                    ct);

            registrationRequest.Confirm(command.AdminUserId, command.Comment);

            await this.UnitOfWork.SaveAsync(ct);

            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                registrationRequest.RegisteredProfileId,
                ct);

            profile.ConfirmRegistration(command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            ProfilesHistory profilesHistory = new(
                registrationRequest.RegisteredProfileId,
                ProfileHistoryAction.ProfileActivated,
                command.AdminUserId);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            ProfileBlobAccessKeyDO blobAccessKey =
                await this.CreateBlobAccessKeyForDocument(
                    registrationRequest.RegistrationRequestId,
                    registrationRequest.RegisteredProfileId,
                    ct);

            profile.AddBlob(
                blobAccessKey.BlobId,
                blobAccessKey.ProfileKeyId,
                blobAccessKey.CreatedByLoginId,
                null,
                blobAccessKey.EncryptedKey,
                ProfileBlobAccessKeyType.Registration);

            await this.UnitOfWork.SaveAsync(ct);

            EmailQueueMessage emailQueueMessage = await this.GetNotification(
                registrationRequest.RegistrationRequestId,
                registrationRequest.RegistrationEmail,
                registrationRequest.CreatedBy,
                profile.ElectronicSubjectName,
                ct);

            await this.QueueMessagesService.PostMessageAsync(
                emailQueueMessage,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return new(true, string.Empty);
        }

        private record ProfileBlobAccessKeyDO(
            int BlobId,
            int ProfileKeyId,
            int CreatedByLoginId,
            byte[] EncryptedKey);

        private async Task<ProfileBlobAccessKeyDO> CreateBlobAccessKeyForDocument(
            int registrationRequestId,
            int profileId,
            CancellationToken ct)
        {
            IAdminRegistrationsEditQueryRepository.GetRegistrationRequestBlobVO blob =
                await this.AdminRegistrationsEditQueryRepository
                    .GetRegistrationRequestBlobAsync(
                        registrationRequestId,
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

            IProfilesService.ProfileKeyVO profileKey =
                await this.ProfilesService.GetOrCreateProfileKeyAndSaveAsync(
                    profileId,
                    ct);

            Keystore.EncryptWithRsaKeyResponse encryptedKeyResp =
                await this.KeystoreClient.EncryptWithRsaKeyAsync(
                    request: new Keystore.EncryptWithRsaKeyRequest
                    {
                        Key = new Keystore.RsaKey
                        {
                            Provider = profileKey.Provider,
                            KeyName = profileKey.KeyName,
                            OaepPadding = profileKey.OaepPadding,
                        },
                        Plaintext = decryptedKeyResp.Plaintext
                    },
                    cancellationToken: ct);

            return new ProfileBlobAccessKeyDO(
                blob.BlobId,
                profileKey.ProfileKeyId,
                blob.CreatedByLoginId,
                encryptedKeyResp.EncryptedData.ToByteArray());
        }

        private async Task<EmailQueueMessage> GetNotification(
            int registrationRequestId,
            string registrationRequestRecipient,
            int registrationRequestCreatedBy,
            string registrationRequestProfile,
            CancellationToken ct)
        {
            string registrationRequestAuthor =
               await this.AdminRegistrationsEditQueryRepository.GetLoginNameAsync(
                   registrationRequestCreatedBy,
                   ct);

            string emailSubect = Notifications.ResourceManager
                .GetString(nameof(Notifications.RegistrationRequestConfirmedEmailSubject))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.RegistrationRequestConfirmedEmailSubject)}");

            string emailBody = Notifications.ResourceManager
                .GetString(nameof(Notifications.RegistrationRequestConfirmedEmailBody))
                    ?? throw new Exception(
                        $"Missing resource {nameof(Notifications.RegistrationRequestConfirmedEmailBody)}");

            string webPortalUrl =
              this.DomainOptionsAccessor.Value.WebPortalUrl
              ?? throw new Exception(
                  $"Missing required option {nameof(DomainOptions.WebPortalUrl)}");

            return new EmailQueueMessage(
                registrationRequestRecipient,
                string.Format(emailSubect, registrationRequestProfile),
                string.Format(
                    emailBody,
                    registrationRequestAuthor,
                    registrationRequestProfile,
                    webPortalUrl),
                false,
                new
                {
                    Event = NotificationEvent,
                    RegistrationRequestId = registrationRequestId
                });
        }
    }
}
