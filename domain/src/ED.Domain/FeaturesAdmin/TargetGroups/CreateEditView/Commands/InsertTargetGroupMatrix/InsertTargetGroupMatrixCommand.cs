using MediatR;

namespace ED.Domain
{
    public record InsertTargetGroupMatrixCommand(
        int TargetGroupId,
        int[] RecipientTargetGroupIds,
        int AdminUserId
    ) : IRequest;
}
