using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record RemoveRecipientGroupMembersCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<RemoveRecipientGroupMembersCommand, Unit>
    {
        public async Task<Unit> Handle(
            RemoveRecipientGroupMembersCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            recipientGroup.RemoveMemberByAdmin(
                command.ProfileId,
                command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
