using MediatR;

namespace ED.Domain
{
    public record DeleteTargetGroupMatrixCommand(
        int TargetGroupId,
        int RecipientTargetGroupId,
        int AdminUserId
    ) : IRequest;
}
