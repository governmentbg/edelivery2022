using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileDataCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        ITargetGroupProfileAggregateRepository TargetGroupProfileAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository)
        : IRequestHandler<UpdateProfileDataCommand, UpdateProfileDataCommandResult>
    {
        public async Task<UpdateProfileDataCommandResult> Handle(
            UpdateProfileDataCommand command,
            CancellationToken ct)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileBasicInfoVO info =
                await this.AdminProfilesCreateEditViewQueryRepository
                    .GetProfileBasicInfoAsync(
                        command.ProfileId,
                        ct);

            if (info.TargetGroupId != command.TargetGroupId)
            {
                if (info.TargetGroupId == TargetGroup.IndividualTargetGroupId)
                {
                    throw new Exception("Changing the target group of individuals is not allowed.");
                }

                if (command.TargetGroupId == TargetGroup.IndividualTargetGroupId)
                {
                    throw new Exception("Changing the target group to individuals is not allowed.");
                }
            }

            if ((command.Identifier != info.Identifier
                || command.TargetGroupId != info.TargetGroupId)
                && info.IsActivated)
            {
                bool hasActiveOrPendingProfile =
                    await this.AdminProfilesCreateEditViewQueryRepository
                        .HasActiveOrPendingProfileAsync(
                            command.ProfileId,
                            command.Identifier,
                            command.TargetGroupId,
                            ct);

                if (hasActiveOrPendingProfile)
                {
                    return new UpdateProfileDataCommandResult(
                        false,
                        "There is an active profile or pending registration with the same identifier.");
                }
            }

            Profile profile =
                await this.ProfileAggregateRepository
                    .FindAsync(command.ProfileId, ct);

            StringBuilder updatesLog = new();

            profile.UpdateDataByAdmin(
                command.Identifier,
                command.EmailAddress,
                command.Phone,
                command.AddressCountryCode,
                command.AddressState,
                command.AddressCity,
                command.AddressResidence,
                command.EnableMessagesWithCode,
                command.AdminUserId,
                updatesLog);

            if (command.IndividualData != null)
            {
                profile.UpdateIndividualNameByAdmin(
                    command.IndividualData.FirstName,
                    command.IndividualData.MiddleName,
                    command.IndividualData.LastName,
                    command.AdminUserId,
                    updatesLog);
            }
            else if (command.LegalEntityData != null)
            {
                profile.UpdateLegalEntityNameByAdmin(
                    command.LegalEntityData.Name,
                    command.AdminUserId,
                    updatesLog);
            }
            else
            {
                throw new Exception("The command should either have an IndividualData or a LegalEntityData.");
            }

            ProfilesHistory profilesHistory = new(
                profile.Id,
                ProfileHistoryAction.ProfileUpdated,
                command.AdminUserId)
            {
                ActionDetails =
                    ProfilesHistory.GenerateAccessDetails(
                        ProfileHistoryAction.ProfileUpdated,
                        profile.ElectronicSubjectId,
                        profile.ElectronicSubjectName,
                        updatesLog.ToString()),
                IPAddress = command.Ip,
            };

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            TargetGroupProfile currentTargetGroup =
                await this.TargetGroupProfileAggregateRepository
                    .FindAsync(info.TargetGroupId, command.ProfileId, ct);

            this.TargetGroupProfileAggregateRepository.Remove(currentTargetGroup);
            await this.TargetGroupProfileAggregateRepository.AddAsync(
                new TargetGroupProfile(command.TargetGroupId, command.ProfileId),
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return new UpdateProfileDataCommandResult(true, string.Empty);
        }
    }
}
