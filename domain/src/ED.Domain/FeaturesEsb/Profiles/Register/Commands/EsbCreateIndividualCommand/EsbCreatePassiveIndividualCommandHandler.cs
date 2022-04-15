using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record EsbCreatePassiveIndividualCommandHandler(
        IUnitOfWork UnitOfWork,
        LoginAggregateRepository LoginAggregateRepository,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IProfileRegisterQueryRepository ProfileRegisterQueryRepository,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<EsbCreatePassiveIndividualCommand, int>
    {
        public async Task<int> Handle(
            EsbCreatePassiveIndividualCommand command,
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

            await using ITransaction transaction =
                await this.UnitOfWork.BeginTransactionAsync(ct);

            Profile profile = Profile.CreateInstanceEsbPassiveRegistration(
                command.FirstName,
                command.MiddleName,
                command.LastName,
                command.Identifier,
                command.Phone,
                command.Email,
                command.Residence,
                command.ActionLoginId,
                newRsaKey.Key.Provider,
                newRsaKey.Key.KeyName,
                newRsaKey.Key.OaepPadding,
                issuedAt,
                expiresAt);

            await this.ProfileAggregateRepository.AddAsync(profile, ct);

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

            await transaction.CommitAsync(ct);

            return profile.Id;
        }
    }
}
