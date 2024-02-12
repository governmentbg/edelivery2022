using MediatR;

namespace ED.Domain
{
    public record UpdateProfileAccessCommand(
        int ProfileId,
        int LoginId,
        string Details,
        int ActionLoginId,
        string Ip,
        GrantProfileAccessCommandPermission[] Permissions)
        : IRequest;

    public record UpdateProfileAccessCommandPermission(
        LoginProfilePermissionType Permission,
        int? TemplateId);
}
