using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateOrUpdateProfileQuotasCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<Profile> ProfileAggregateRepository,
        IAggregateRepository<ProfilesHistory> ProfilesHistoryAggregateRepository)
        : IRequestHandler<CreateOrUpdateProfileQuotasCommand, Unit>
    {
        private const string ActionDetails = "Change quotas by admin";

        public async Task<Unit> Handle(
            CreateOrUpdateProfileQuotasCommand command,
            CancellationToken ct)
        {
            Profile profile =
                await this.ProfileAggregateRepository
                    .FindAsync(command.ProfileId, ct);

            profile.CreateOrUpdateQuotas(
                command.StorageQuotaInMb,
                command.AdminUserId);

            ProfilesHistory profilesHistory = ProfilesHistory.CreateInstanceByAdmin(
                profile.Id,
                ProfileHistoryAction.ProfileUpdated,
                command.AdminUserId,
                ProfilesHistory.GenerateAccessDetails(
                    ProfileHistoryAction.ProfileUpdated,
                    profile.ElectronicSubjectId,
                    profile.ElectronicSubjectName,
                    CreateOrUpdateProfileQuotasCommandHandler.ActionDetails),
                command.Ip);

            await this.ProfilesHistoryAggregateRepository.AddAsync(
                profilesHistory,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
