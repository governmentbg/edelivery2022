using MediatR;

namespace ED.Domain
{
    public record GrantProfileAccessByAdminCommand(
        int ProfileId,
        int LoginId,
        int AdminUserId,
        string IP,
        GrantProfileAccessByAdminCommandPermission[] Permissions)
        : IRequest;

    public record GrantProfileAccessByAdminCommandPermission(
        LoginProfilePermissionType Permission,
        int? TemplateId);
}
