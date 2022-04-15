using MediatR;

namespace ED.Domain
{
    public record DeleteProfileRecipientGroupMemberCommand(
        int RecipientGroupId,
        int ProfileId,
        int LoginId)
        : IRequest<Unit>;
}
