using MediatR;

namespace ED.Domain
{
    public record ArchiveProfileRecipientGroupCommand(
        int RecipientGroupId,
        int LoginId)
        : IRequest<Unit>;
}
