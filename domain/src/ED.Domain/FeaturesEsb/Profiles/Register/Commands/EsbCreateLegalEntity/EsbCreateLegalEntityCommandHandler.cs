using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record EsbCreateLegalEntityCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IEsbProfilesRegisterQueryRepository EsbProfilesRegisterQueryRepository,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<EsbCreateLegalEntityCommand, int>
    {
        private const string RegisterLegalEntityActionDetails = "Register legal entity";

        public async Task<int> Handle(
            EsbCreateLegalEntityCommand command,
            CancellationToken ct)
        {
            // create the key upfront to minimize the transaction lifetime
            Keystore.CreateRsaKeyResponse newRsaKey =
                await this.KeystoreClient.CreateRsaKeyAsync(
                    request: new Empty(),
                    cancellationToken: ct);

            DateTime issuedAt = DateTime.Now;
            DateTime expiresAt =
                issuedAt.Add(this.DomainOptionsAccessor.Value.ProfileKeyExpiration);

            IEsbProfilesRegisterQueryRepository.GetLoginsByIdentifiersVO[] loginData =
                await this.EsbProfilesRegisterQueryRepository.GetLoginsByIdentifiersAsync(
                    command.OwnersData.Select(e => e.Identifier).ToArray(),
                    ct);

            await using ITransaction transaction =
                   await this.UnitOfWork.BeginTransactionAsync(ct);

            Profile profile = Profile.CreateInstanceEsbLegalEntityRegistration(
                command.Identifier,
                command.Name,
                command.Phone,
                command.Email,
                command.Address.Residence,
                command.Address.City,
                command.Address.State,
                command.Address.CountryIso,
                loginData
                    .Select(e =>
                    {
                        EsbCreateLegalEntityCommandOwnerData match =
                            command.OwnersData.First(d => d.Identifier == e.Identifier);

                        return (e.LoginId, match.Email, match.Phone);
                    })
                    .ToArray(),
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
                    RegisterLegalEntityActionDetails),
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistoryCreate,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await this.TargetGroupProfileAggregateRepository.AddAsync(
                new TargetGroupProfile(command.TargetGroupId, profile.Id), ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return profile.Id;
        }
    }
}
