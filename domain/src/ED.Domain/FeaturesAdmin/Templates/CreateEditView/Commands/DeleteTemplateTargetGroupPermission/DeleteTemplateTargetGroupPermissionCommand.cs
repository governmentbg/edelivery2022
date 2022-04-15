using MediatR;

namespace ED.Domain
{
    public record DeleteTemplateTargetGroupPermissionCommand(
        int TemplateId,
        int TargetGroupId
    ) : IRequest;
}
