using MediatR;

namespace ED.Domain
{
    public record MarkProfileAsNonReadonlyCommand(
        int ProfileId,
        int AdminUserId,
        string Ip)
        : IRequest;
}
