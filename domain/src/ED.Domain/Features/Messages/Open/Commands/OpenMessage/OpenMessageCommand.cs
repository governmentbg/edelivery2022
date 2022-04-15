using MediatR;

namespace ED.Domain
{
    public record OpenMessageCommand(
        int MessageId,
        int ProfileId,
        int LoginId)
        : IRequest<int>;
}
