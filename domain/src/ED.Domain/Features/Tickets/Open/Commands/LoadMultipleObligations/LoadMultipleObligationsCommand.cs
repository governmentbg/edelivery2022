using MediatR;

namespace ED.Domain
{
    public record LoadMultipleObligationsCommand(
        int ProfileId)
        : IRequest<LoadMultipleObligationsCommandResult>;

    public record LoadMultipleObligationsCommandResult(
            int Count,
            string? NotFoundMessage);
}
