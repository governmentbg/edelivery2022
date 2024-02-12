using MediatR;

namespace ED.Domain
{
    public record GetSingleObligationAccessCodeCommand(
        int ProfileId,
        string DocumentType,
        string DocumentIdentifier)
        : IRequest<GetSingleObligationAccessCodeCommandResult>;

    public record GetSingleObligationAccessCodeCommandResult(
            string? AccessCode,
            string? NotFoundMessage);
}
