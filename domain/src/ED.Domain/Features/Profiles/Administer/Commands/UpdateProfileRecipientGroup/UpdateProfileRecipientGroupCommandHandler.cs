using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record UpdateProfileRecipientGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<UpdateProfileRecipientGroupCommand, Unit>
    {
        public async Task<Unit> Handle(
            UpdateProfileRecipientGroupCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            recipientGroup.Update(
                command.Name,
                command.LoginId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
