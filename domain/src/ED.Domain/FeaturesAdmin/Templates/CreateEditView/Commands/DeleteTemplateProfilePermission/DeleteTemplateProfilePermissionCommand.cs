using MediatR;

namespace ED.Domain
{
    public record DeleteTemplateProfilePermissionCommand(
        int TemplateId,
        int ProfileId
    ) : IRequest;
}
