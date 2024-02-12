using MediatR;

namespace ED.Domain
{
    public record OpenTicketCommand(
        int MessageId,
        int ProfileId,
        int LoginId)
        : IRequest<int>;
}
