using MediatR;

namespace ED.Domain
{
    public record GrantProfileAccessCommand(
        int ProfileId,
        int LoginId,
        bool IsDefault,
        bool IsEmailNotificationEnabled,
        bool IsEmailNotificationOnDeliveryEnabled,
        bool IsSmsNotificationEnabled,
        bool IsSmsNotificationOnDeliveryEnabled,
        bool IsViberNotificationEnabled,
        bool IsViberNotificationOnDeliveryEnabled,
        string Details,
        int ActionLoginId,
        string Ip,
        GrantProfileAccessCommandPermission[] Permissions)
        : IRequest;

    public record GrantProfileAccessCommandPermission(
        LoginProfilePermissionType Permission,
        int? TemplateId);
}
