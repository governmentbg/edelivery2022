using MediatR;

namespace ED.Domain
{
    public record ArchiveTargetGroupCommand(
        int TargetGroupId,
        int AdminUserId
    ) : IRequest;
}
