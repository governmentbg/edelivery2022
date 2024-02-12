using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record CreateOrUpdateIndividualCommandHandler(
        IUnitOfWork UnitOfWork,
        LoginAggregateRepository LoginAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IProfileRegisterQueryRepository ProfileRegisterQueryRepository,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<CreateOrUpdateIndividualCommand, CreateOrUpdateIndividualCommandResult>
    {
        private const string RegisterIndividualActionDetails = "Register individual";

        public async Task<CreateOrUpdateIndividualCommandResult> Handle(
            CreateOrUpdateIndividualCommand command,
            CancellationToken ct)
        {
            IProfileRegisterQueryRepository.GetExistingIndividualVO? existingIndividual =
                await this.ProfileRegisterQueryRepository.GetExistingIndividualAsync(
                    command.Identifier,
                    ct);

            if (existingIndividual != null
                && (!existingIndividual.IsPassive || existingIndividual.IsRegistered))
            {
                throw new ArgumentException($"{nameof(command.Identifier)} is already used");
            }

            Profile profile;

            if (existingIndividual != null && existingIndividual.IsPassive)
            {
                profile = await this.ProfileAggregateRepository.FindAsync(
                    existingIndividual.ProfileId,
                    ct);

                await using ITransaction transaction =
                    await this.UnitOfWork.BeginTransactionAsync(ct);

                profile.UpdateIndividual(
                    command.FirstName,
                    command.MiddleName,
                    command.LastName,
                    command.Phone,
                    command.Email,
                    command.IsPassive,
                    command.Residence,
                    command.ActionLoginId);

                await this.UnitOfWork.SaveAsync(ct);

                Login login = new(
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    profile.EmailAddress,
                    profile.Phone,
                    profile.Identifier);

                await this.LoginAggregateRepository.AddAsync(login, ct);

                await this.UnitOfWork.SaveAsync(ct);

                if (!command.IsPassive)
                {
                    profile.GrantLoginAccess(
                        login.Id,
                        true,
                        command.IsEmailNotificationEnabled,
                        false,
                        command.IsSsmsNotificationEnabled,
                        false,
                        command.IsViberNotificationEnabled,
                        false,
                        profile.EmailAddress,
                        profile.Phone,
                        command.ActionLoginId,
                        new[]
                        {
                            (LoginProfilePermissionType.OwnerAccess, (int?)null)
                        });

                    ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByLogin(
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
                        profilesHistory,
                        ct);

                    await this.UnitOfWork.SaveAsync(ct);
                }

                await transaction.CommitAsync(ct);
            }
            else
            {
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

                profile = new(
                    command.FirstName,
                    command.MiddleName,
                    command.LastName,
                    command.Identifier,
                    command.Phone,
                    command.Email,
                    command.Residence,
                    command.IsPassive,
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

                Login login = new(
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    profile.EmailAddress,
                    profile.Phone,
                    profile.Identifier);

                await this.LoginAggregateRepository.AddAsync(login, ct);

                await this.UnitOfWork.SaveAsync(ct);

                if (!command.IsPassive)
                {
                    profile.GrantLoginAccess(
                        login.Id,
                        true,
                        command.IsEmailNotificationEnabled,
                        false,
                        command.IsSsmsNotificationEnabled,
                        false,
                        command.IsViberNotificationEnabled,
                        false,
                        profile.EmailAddress,
                        profile.Phone,
                        Login.SystemLoginId,
                        new[]
                        {
                            (LoginProfilePermissionType.OwnerAccess, (int?)null)
                        });

                    ProfilesHistory profilesHistoryGrantAccess = ProfilesHistory.CreateInstanceByLogin(
                        profile.Id,
                        ProfileHistoryAction.GrantAccessToProfile,
                        Login.SystemLoginId,
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
                }

                await transaction.CommitAsync(ct);
            }

            return new CreateOrUpdateIndividualCommandResult(
                profile.Id,
                profile.ElectronicSubjectId,
                profile.ElectronicSubjectName);
        }
    }
}
