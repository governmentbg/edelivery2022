using MediatR;

namespace ED.Domain
{
    public record UpdateProfileRecipientGroupMembersCommand(
        int RecipientGroupId,
        int[] ProfileIds,
        int LoginId,
        int ProfileId)
        : IRequest<Unit>;
}
