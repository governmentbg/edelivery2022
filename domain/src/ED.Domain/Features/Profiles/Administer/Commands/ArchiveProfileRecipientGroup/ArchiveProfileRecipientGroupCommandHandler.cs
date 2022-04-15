using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record ArchiveProfileRecipientGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<ArchiveProfileRecipientGroupCommand, Unit>
    {
        public async Task<Unit> Handle(
            ArchiveProfileRecipientGroupCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup =
                await this.RecipientGroupAggregateRepository.FindAsync(
                    command.RecipientGroupId,
                    ct);

            recipientGroup.Archive(command.LoginId);

            await this.UnitOfWork.SaveAsync(ct);

            return default;
        }
    }
}
