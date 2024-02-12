using MediatR;

namespace ED.Domain
{
    public record RevokeProfileAccessCommand(
        int ProfileId,
        int LoginId,
        int ActionLoginId,
        string Ip)
        : IRequest;
}
