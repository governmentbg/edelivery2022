using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateRecipientGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository)
        : IRequestHandler<CreateRecipientGroupCommand, int>
    {
        public async Task<int> Handle(
            CreateRecipientGroupCommand command,
            CancellationToken ct)
        {
            RecipientGroup recipientGroup = new(
                command.Name,
                command.AdminUserId);

            await this.RecipientGroupAggregateRepository.AddAsync(
                recipientGroup,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return recipientGroup.RecipientGroupId;
        }
    }
}
