using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ED.Domain
{
    internal record CreateProfileRecipientGroupCommandHandler(
        IUnitOfWork UnitOfWork,
        IAggregateRepository<RecipientGroup> RecipientGroupAggregateRepository,
        IProfileAdministerQueryRepository ProfileAdministerQueryRepository)
        : IRequestHandler<CreateProfileRecipientGroupCommand, CreateProfileRecipientGroupCommandResult>
    {
        public async Task<CreateProfileRecipientGroupCommandResult> Handle(
            CreateProfileRecipientGroupCommand command,
            CancellationToken ct)
        {
            bool hasExistingGroupName =
                await this.ProfileAdministerQueryRepository.HasExistingGroupName(
                    command.Name,
                    command.ProfileId,
                    ct);

            if (hasExistingGroupName)
            {
                return new CreateProfileRecipientGroupCommandResult(
                    0,
                    false,
                    "There is already a group with this name");
            }

            RecipientGroup recipientGroup = new(
                    command.Name,
                    command.ProfileId,
                    command.LoginId);

            await this.RecipientGroupAggregateRepository.AddAsync(
                recipientGroup,
                ct);

            await this.UnitOfWork.SaveAsync(ct);

            return new CreateProfileRecipientGroupCommandResult(
                recipientGroup.RecipientGroupId,
                true,
                string.Empty);
        }
    }
}
