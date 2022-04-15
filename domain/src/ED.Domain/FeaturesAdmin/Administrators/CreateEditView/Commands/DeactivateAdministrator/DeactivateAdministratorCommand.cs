using MediatR;

namespace ED.Domain
{
    public record DeactivateAdministratorCommand(
        int Id,
        int AdminUserId)
        : IRequest;
}
