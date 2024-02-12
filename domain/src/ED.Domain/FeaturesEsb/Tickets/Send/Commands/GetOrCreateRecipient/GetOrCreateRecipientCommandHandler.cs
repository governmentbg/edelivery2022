using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MediatR;
using Microsoft.Extensions.Options;

namespace ED.Domain
{
    internal record GetOrCreateRecipientCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<TargetGroupProfile> TargetGroupProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        ED.Keystore.Keystore.KeystoreClient KeystoreClient,
        IEsbTicketsSendQueryRepository EsbTicketsSendQueryRepository,
        IOptions<DomainOptions> DomainOptionsAccessor)
        : IRequestHandler<GetOrCreateRecipientCommand, GetOrCreateRecipientCommandResult>
    {
        private const string RegisterPassiveIndividualActionDetails = "Register passive individual - ticket";

        public async Task<GetOrCreateRecipientCommandResult> Handle(
            GetOrCreateRecipientCommand command,
            CancellationToken ct)
        {
            if (command.LegalEntity != null)
            {
                int? anyLegalEntityProfileId =
                    await this.EsbTicketsSendQueryRepository.GetAnyLegalEntityProfileIdAsync(
                        command.LegalEntity.Identifier,
                        new int[] {
                            TargetGroup.LegalEntityTargetGroupId,
                            TargetGroup.PublicAdministrationTargetGroupId,
                            TargetGroup.SocialOrganizationTargetGroupId,
                        },
                        ct);

                if (anyLegalEntityProfileId.HasValue)
                {
                    return new GetOrCreateRecipientCommandResult(
                        anyLegalEntityProfileId.Value,
                        command.LegalEntity.Identifier,
                        false);
                }
            }

            int? individualProfileId =
                await this.EsbTicketsSendQueryRepository.GetIndividualProfileIdAsync(
                    command.Individual.Identifier,
                    ct);

            if (individualProfileId.HasValue)
            {
                return new GetOrCreateRecipientCommandResult(
                    individualProfileId.Value,
                    command.Individual.Identifier,
                    true);
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

            Profile profile = Profile.CreateInstanceEsbTicketPassiveIndividualRegistration(
                command.Individual.Identifier,
                command.Individual.FirstName,
                command.Individual.MiddleName ?? string.Empty,
                command.Individual.LastName,
                command.Individual.Phone,
                command.Individual.Email,
                command.ActionLoginId,
                newRsaKey.Key.Provider,
                newRsaKey.Key.KeyName,
                newRsaKey.Key.OaepPadding,
                issuedAt,
                expiresAt);

            await this.ProfileAggregateRepository.AddAsync(profile, ct);

            await this.UnitOfWork.SaveAsync(ct);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByLogin(
                profile.Id,
                ProfileHistoryAction.CreateProfile,
                command.ActionLoginId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.CreateProfile,
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    RegisterPassiveIndividualActionDetails),
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await this.TargetGroupProfileAggregateRepository.AddAsync(
                new TargetGroupProfile(
                    TargetGroup.IndividualTargetGroupId,
                    profile.Id),
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            await transaction.CommitAsync(ct);

            return new GetOrCreateRecipientCommandResult(
                profile.Id,
                profile.Identifier,
                true);
        }
    }
}
