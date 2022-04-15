using MediatR;

namespace ED.Domain
{
    public record CreateRecipientGroupCommand(
        string Name,
        int AdminUserId
    ) : IRequest<int>;
}
