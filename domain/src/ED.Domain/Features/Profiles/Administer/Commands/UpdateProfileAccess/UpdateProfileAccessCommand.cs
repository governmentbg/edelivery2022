using MediatR;

namespace ED.Domain
{
    public record UpdateProfileAccessCommand(
        int ProfileId,
        int LoginId,
        string Details,
        int ActionLoginId,
        string IP,
        GrantProfileAccessCommandPermission[] Permissions)
        : IRequest;

    public record UpdateProfileAccessCommandPermission(
        LoginProfilePermissionType Permission,
        int? TemplateId);
}
