using MediatR;

namespace ED.Domain
{
    public record ChangePasswordAdminProfileCommand(
        int Id,
        string PasswordHash
    ) : IRequest;
}
