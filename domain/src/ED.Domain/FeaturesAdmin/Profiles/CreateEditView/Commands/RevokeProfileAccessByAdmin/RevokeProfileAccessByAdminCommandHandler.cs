using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RevokeProfileAccessByAdminCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<RevokeProfileAccessByAdminCommand, Unit>
    {
        public async Task<Unit> Handle(
            RevokeProfileAccessByAdminCommand command,
            CancellationToken ct)
        {
            Profile profile = await this.ProfileAggregateRepository.FindAsync(
                command.ProfileId,
                ct);

            IAdminProfilesCreateEditViewQueryRepository.GetLoginInfoVO loginInfo =
                await this.AdminProfilesCreateEditViewQueryRepository.GetLoginInfoAsync(
                    command.LoginId,
                    profile.ElectronicSubjectId,
                    ct);

            profile.RevokeAccess(command.LoginId);

            ProfilesHistory profilesHistory = new(
               profile.Id,
               ProfileHistoryAction.RemoveAccessToProfile,
               command.AdminUserId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.RemoveAccessToProfile,
                    loginInfo.ElectronicSubjectId,
                    loginInfo.ElectronicSubjectName,
                    string.Empty),
               command.IP);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
