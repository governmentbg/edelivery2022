using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record EditRecipientGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<EditRecipientGroupCommand, Unit>
    {
        public async Task<Unit> Handle(
            EditRecipientGroupCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            recipientGroup.UpdateByAdmin(command.Name, command.AdminUserId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
