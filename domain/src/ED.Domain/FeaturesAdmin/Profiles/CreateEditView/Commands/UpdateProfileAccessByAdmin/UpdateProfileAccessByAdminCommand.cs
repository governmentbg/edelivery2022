using MediatR;

namespace ED.Domain
{
    public record UpdateProfileAccessByAdminCommand(
        int ProfileId,
        int LoginId,
        int AdminUserId,
        string IP,
        UpdateProfileAccessByAdminCommandPermission[] Permissions)
        : IRequest;

    public record UpdateProfileAccessByAdminCommandPermission(
        LoginProfilePermissionType Permission,
        int? TemplateId);
}
