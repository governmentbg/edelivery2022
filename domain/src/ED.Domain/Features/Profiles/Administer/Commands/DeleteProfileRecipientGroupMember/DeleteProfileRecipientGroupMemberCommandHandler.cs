using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record DeleteProfileRecipientGroupMemberCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<DeleteProfileRecipientGroupMemberCommand, Unit>
    {
        public async Task<Unit> Handle(
            DeleteProfileRecipientGroupMemberCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            recipientGroup.RemoveMember(
                command.ProfileId,
                command.LoginId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
