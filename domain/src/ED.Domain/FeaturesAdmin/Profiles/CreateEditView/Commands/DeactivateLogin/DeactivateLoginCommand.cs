using MediatR;

namespace ED.Domain
{
    public record DeactivateLoginCommand(int LoginId) : IRequest;
}
