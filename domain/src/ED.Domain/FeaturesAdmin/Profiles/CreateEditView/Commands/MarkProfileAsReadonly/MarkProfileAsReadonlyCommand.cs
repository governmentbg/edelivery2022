using MediatR;

namespace ED.Domain
{
    public record MarkProfileAsReadonlyCommand(
        int ProfileId,
        int AdminUserId,
        string Ip)
        : IRequest;
}
