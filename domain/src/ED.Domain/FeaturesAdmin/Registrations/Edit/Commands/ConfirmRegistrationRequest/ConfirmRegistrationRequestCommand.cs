using MediatR;

namespace ED.Domain
{
    public record ConfirmRegistrationRequestCommand(
        int AdminUserId,
        int RegistrationRequestId,
        string Comment)
        : IRequest<ConfirmRegistrationRequestCommandResult>;

    public record ConfirmRegistrationRequestCommandResult(
        bool IsSuccessful,
        string Error);
}
