using MediatR;

namespace ED.Domain
{
    public record CreateTargetGroupCommand(
        string Name,
        int AdminUserId
    ) : IRequest<int>;
}
