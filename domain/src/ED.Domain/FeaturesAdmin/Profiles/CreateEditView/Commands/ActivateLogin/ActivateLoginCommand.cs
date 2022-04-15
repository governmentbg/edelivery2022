using MediatR;

namespace ED.Domain
{
    public record ActivateLoginCommand(int LoginId) : IRequest;
}
