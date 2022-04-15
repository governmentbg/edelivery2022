using MediatR;

namespace ED.Domain
{
    public record AddRecipientGroupMembersCommand(
        int RecipientGroupId,
        int[] ProfileIds,
        int AdminUserId
    ) : IRequest;
}
