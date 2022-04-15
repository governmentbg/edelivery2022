using MediatR;

namespace ED.Domain
{
    public record CreateRegisterRequestCommand(
        string RegistrationEmail,
        string RegistrationPhone,
        bool RegistrationIsEmailNotificationEnabled,
        bool RegistrationIsSMSNotificationEnabled,
        bool RegistrationIsViberNotificationEnabled,
        string Name,
        string Identifier,
        string Phone,
        string Email,
        string Residence,
        string? City,
        string? State,
        string? Country,
        int TargetGroupId,
        int BlobId,
        int LoginId)
        : IRequest<CreateRegisterRequestCommandResult>;

    public record CreateRegisterRequestCommandResult(
        bool IsSuccessful,
        string Error);
}
