using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record EsbCreateOrUpdateIndividualCommandHandler(
        IUnitOfWork UnitOfWork,
        LoginAggregateRepository LoginAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IEsbProfilesRegisterQueryRepository EsbProfilesRegisterQueryRepository,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<EsbCreateOrUpdateIndividualCommand, int>
    {
        private const string RegisterIndividualActionDetails = "Register individual";

        public async Task<int> Handle(
            EsbCreateOrUpdateIndividualCommand command,
            CancellationToken ct)
        {
            IEsbProfilesRegisterQueryRepository.GetExistingIndividualVO? existingIndividual =
                await this.EsbProfilesRegisterQueryRepository.GetExistingIndividualAsync(
                    command.Identifier,
                    ct);

            if (existingIndividual != null
               && (!existingIndividual.IsPassive || existingIndividual.IsRegistered))
            {
                throw new ArgumentException($"{nameof(command.Identifier)} is already used");
            }

            Profile profile = null!;
            Keystore.CreateRsaKeyResponse newRsaKey = null!;

            // transaction prerequisites
            if (existingIndividual != null && existingIndividual.IsPassive)
            {
                profile = await this.ProfileAggregateRepository.FindAsync(
                   existingIndividual.ProfileId,
                   ct);
            }
            else
            {
                newRsaKey =
                    await this.KeystoreClient.CreateRsaKeyAsync(
                        request: new Empty(),
                        cancellationToken: ct);
            }

            // transaction
            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            if (existingIndividual != null && existingIndividual.IsPassive)
            {
                profile!.UpdateIndividual(
                    command.FirstName,
                    command.MiddleName,
                    command.LastName,
                    command.Phone,
                    command.Email,
                    isPassive: !command.IsFullFeatured,
                    command.Address.Residence,
                    command.Address.City,
                    command.Address.State,
                    command.Address.CountryIso,
                    command.ActionLoginId);

                await this.UnitOfWork.SaveAsync(ct);
            }
            else
            {
                DateTime issuedAt = DateTime.Now;
                DateTime expiresAt =
                    issuedAt.Add(this.DomainOptionsAccessor.Value.ProfileKeyExpiration);

                profile = Profile.CreateInstanceEsbIndividualRegistration(
                    command.Identifier,
                    command.FirstName,
                    command.MiddleName,
                    command.LastName,
                    command.Phone,
                    command.Email,
                    command.Address.Residence,
                    command.Address.City,
                    command.Address.State,
                    command.Address.CountryIso,
                    command.IsFullFeatured,
                    command.ActionLoginId,
                    newRsaKey.Key.Provider,
                    newRsaKey.Key.KeyName,
                    newRsaKey.Key.OaepPadding,
                    issuedAt,
                    expiresAt);

                await this.ProfileAggregateRepository.AddAsync(profile, ct);

                await this.UnitOfWork.SaveAsync(ct);

                ProfilesHistory profilesHistoryCreate = ProfilesHistory.CreateInstanceByLogin(
                    profile.Id,
                    ProfileHistoryAction.CreateProfile,
                    command.ActionLoginId,
                    ProfilesHistory.GenerateAccessDetails(
                        ProfileHistoryAction.CreateProfile,
                        profile.ElectronicSubjectId,
                        profile.ElectronicSubjectName,
                        RegisterIndividualActionDetails),
                    command.Ip);

                await this.ProfilesHistoryAggregateRepository.AddAsync(
                    profilesHistoryCreate,
                    ct);

                await this.UnitOfWork.SaveAsync(ct);

                await this.TargetGroupProfileAggregateRepository.AddAsync(
                    new TargetGroupProfile(
                        TargetGroup.IndividualTargetGroupId,
                        profile.Id),
                    ct);

                await this.UnitOfWork.SaveAsync(ct);
            }

            Login login = new(
                profile.ElectronicSubjectId,
                profile.ElectronicSubjectName,
                profile.EmailAddress,
                profile.Phone,
                profile.Identifier);

            await this.LoginAggregateRepository.AddAsync(login, ct);

            await this.UnitOfWork.SaveAsync(ct);

            profile.GrantLoginAccess(
                login.Id,
                true,
                isEmailNotificationEnabled: command.IsEmailNotificationEnabled,
                isEmailNotificationOnDeliveryEnabled: false,
                isSmsNotificationEnabled: false,
                isSmsNotificationOnDeliveryEnabled: false,
                isViberNotificationEnabled: false,
                isViberNotificationOnDeliveryEnabled: false,
                profile.EmailAddress,
                profile.Phone,
                command.ActionLoginId,
                new[]
                {
                    (LoginProfilePermissionType.OwnerAccess, (int?)null)
                });

            ProfilesHistory profilesHistoryGrantAccess = ProfilesHistory.CreateInstanceByLogin(
                profile.Id,
                ProfileHistoryAction.GrantAccessToProfile,
                command.ActionLoginId,
                ProfilesHistory.GenerateAccessDetails(
                   ProfileHistoryAction.GrantAccessToProfile,
                   login.ElectronicSubjectId,
                   login.ElectronicSubjectName,
                   string.Empty),
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistoryGrantAccess,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return profile.Id;
        }
    }
}
