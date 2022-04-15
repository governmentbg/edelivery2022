using MediatR;

namespace ED.Domain
{
    public record ActivateAdministratorCommand(
        int Id)
        : IRequest;
}
