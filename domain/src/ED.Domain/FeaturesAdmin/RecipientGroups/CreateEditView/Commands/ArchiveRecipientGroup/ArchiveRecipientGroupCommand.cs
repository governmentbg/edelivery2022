using MediatR;

namespace ED.Domain
{
    public record ArchiveRecipientGroupCommand(
        int RecipientGroupId,
        int AdminUserId
    ) : IRequest;
}
