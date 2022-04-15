using MediatR;

namespace ED.Domain
{
    public record EditTargetGroupCommand(
        int TargetGroupId,
        string Name,
        int AdminUserId
    ) : IRequest;
}
