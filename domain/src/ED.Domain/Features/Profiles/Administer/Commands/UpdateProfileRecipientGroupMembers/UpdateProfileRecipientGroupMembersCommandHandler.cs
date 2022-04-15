using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileRecipientGroupMembersCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository,
        IAdminProfilesCreateEditViewQueryRepository AdminProfilesCreateEditViewQueryRepository)
        : IRequestHandler<UpdateProfileRecipientGroupMembersCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateProfileRecipientGroupMembersCommand command,
            CancellationToken ct)
        {
            IAdminProfilesCreateEditViewQueryRepository.GetProfileBasicInfoVO info =
                await this.AdminProfilesCreateEditViewQueryRepository
                    .GetProfileBasicInfoAsync(
                        command.ProfileId,
                        ct);

            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            if (recipientGroup.ProfileId != command.ProfileId)
            {
                throw new Exception("Can't modify this recipient group");
            }

            if (info.TargetGroupId == TargetGroup.IndividualTargetGroupId)
            {
                recipientGroup.AddMembersWithLimit(
                    command.ProfileIds,
                    command.LoginId);
            }
            else
            {
                recipientGroup.AddMembers(command.ProfileIds, command.LoginId);
            }

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
