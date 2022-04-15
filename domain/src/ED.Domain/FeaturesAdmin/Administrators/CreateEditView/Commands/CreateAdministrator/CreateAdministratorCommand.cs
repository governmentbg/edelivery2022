using MediatR;

namespace ED.Domain
{
    public record CreateAdministratorCommand(
        string FirstName,
        string MiddleName,
        string LastName,
        string Identifier,
        string Phone,
        string Email,
        string UserName,
        string PasswordHash,
        int AdminUserId)
        : IRequest<CreateAdministratorCommandResult>;

    public record CreateAdministratorCommandResult(
        bool IsSuccessful,
        int? Id,
        string Error);
}
