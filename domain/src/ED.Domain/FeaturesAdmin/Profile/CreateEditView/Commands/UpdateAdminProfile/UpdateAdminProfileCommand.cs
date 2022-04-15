using MediatR;

namespace ED.Domain
{
    public record UpdateAdminProfileCommand(
        int Id,
        string FirstName,
        string MiddleName,
        string LastName,
        string Identifier,
        string Phone,
        string Email
    ) : IRequest;
}
