using MediatR;

namespace ED.Domain
{
    public record RemoveRecipientGroupMembersCommand(
        int RecipientGroupId,
        int ProfileId,
        int AdminUserId
    ) : IRequest;
}
