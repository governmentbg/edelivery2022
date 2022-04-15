using MediatR;

namespace ED.Domain
{
    public record RejectRegistrationRequestCommand(
        int AdminUserId,
        int RegistrationRequestId,
        string Comment)
        : IRequest;
}
