using MediatR;

namespace ED.Domain
{
    public record CreateOrUpdateTemplatePermissionsCommand(
        int TemplateId,
        int[] ProfileIds,
        int[] TargetGroupIds,
        bool CanSend,
        bool CanReceive
    ) : IRequest;
}
