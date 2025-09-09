using MediatR;

namespace ED.Domain
{
    public record OpenTicketCommand1(
        int MessageId,
        int ProfileId,
        int LoginId)
        : IRequest<bool>;
}
