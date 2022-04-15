using MediatR;

namespace ED.Domain
{
    public record EditRecipientGroupCommand(
        int RecipientGroupId,
        string Name,
        int AdminUserId
    ) : IRequest;
}
