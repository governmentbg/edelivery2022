using MediatR;

namespace ED.Domain
{
    public record UpdateProfileRecipientGroupCommand(
        int RecipientGroupId,
        string Name,
        int LoginId)
        : IRequest<Unit>;
}
