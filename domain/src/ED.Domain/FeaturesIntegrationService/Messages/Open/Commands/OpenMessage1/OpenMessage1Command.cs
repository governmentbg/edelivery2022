using MediatR;

namespace ED.Domain
{
    public record OpenMessage1Command(
        int MessageId,
        int ProfileId,
        int LoginId,
        string OpenEvent)
        : IRequest<bool>;
}
