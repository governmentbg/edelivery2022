using MediatR;

namespace ED.Domain
{
    public record UpdateProfileAccessByAdminCommand(
        int ProfileId,
        int LoginId,
        int AdminUserId,
        string Ip,
        UpdateProfileAccessByAdminCommandPermission[] Permissions)
        : IRequest;

    public record UpdateProfileAccessByAdminCommandPermission(
        LoginProfilePermissionType Permission,
        int? TemplateId);
}
