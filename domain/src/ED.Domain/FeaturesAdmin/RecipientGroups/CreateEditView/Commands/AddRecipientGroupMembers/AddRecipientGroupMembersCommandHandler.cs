using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record AddRecipientGroupMembersCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<AddRecipientGroupMembersCommand, Unit>
    {
        public async Task<Unit> Handle(
            AddRecipientGroupMembersCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            recipientGroup.AddMembersByAdmin(
                command.ProfileIds,
                command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
