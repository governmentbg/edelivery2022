using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record CreateRegisterRequestCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<RegistrationRequest> RegistrationRequestAggregateRepository,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IProfileRegisterQueryRepository ProfileRegisterQueryRepository,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<CreateRegisterRequestCommand, CreateRegisterRequestCommandResult>
    {
        public async Task<CreateRegisterRequestCommandResult> Handle(
            CreateRegisterRequestCommand command,
            CancellationToken ct)
        {
            bool hasExistingLegalEntity =
                await this.ProfileRegisterQueryRepository.HasExistingLegalEntityAsync(
                    command.Identifier,
                    ct);

            if (hasExistingLegalEntity)
            {
                return new CreateRegisterRequestCommandResult(
                    false,
                    "There is an active or pending for registration legal entity with the same identifier.");
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
                command.City,
                command.State,
                command.Country,
                command.LoginId,
                newRsaKey.Key.Provider,
                newRsaKey.Key.KeyName,
                newRsaKey.Key.OaepPadding,
                issuedAt,
                expiresAt,
                command.RegistrationIsEmailNotificationEnabled,
                false,
                command.RegistrationIsSMSNotificationEnabled,
                false,
                command.RegistrationIsViberNotificationEnabled,
                false,
                command.RegistrationEmail,
                command.RegistrationPhone,
                new (LoginProfilePermissionType permission, int? templateId)[] {
                    (LoginProfilePermissionType.FullMessageAccess, null),
                    (LoginProfilePermissionType.AdministerProfileAccess, null),
                });

            await this.ProfileAggregateRepository.AddAsync(profile, ct);

            await this.UnitOfWork.SaveAsync(ct);

            // TODO: is part of profile aggregate
            await this.TargetGroupProfileAggregateRepository.AddAsync(
                new TargetGroupProfile(command.TargetGroupId, profile.Id), ct);

            await this.UnitOfWork.SaveAsync(ct);

            await this.RegistrationRequestAggregateRepository.AddAsync(
                new RegistrationRequest(
                    profile.Id,
                    command.RegistrationEmail,
                    command.RegistrationPhone,
                    command.BlobId,
                    command.LoginId),
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return new CreateRegisterRequestCommandResult(true, string.Empty);
        }
    }
}
