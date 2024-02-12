using MediatR;

namespace ED.Domain
{
    public record GetOrCreateRecipientCommand(
        GetOrCreateRecipientCommandLegalEntity? LegalEntity,
        GetOrCreateRecipientCommandIndividual Individual,
        int ActionLoginId,
        string Ip)
        : IRequest<GetOrCreateRecipientCommandResult>;

    public record GetOrCreateRecipientCommandLegalEntity(
        string Identifier);

    public record GetOrCreateRecipientCommandIndividual(
        string Identifier,
        string FirstName,
        string? MiddleName,
        string LastName,
        string? Email,
        string? Phone);

    public record GetOrCreateRecipientCommandResult(
        int ProfileId,
        string Identifier,
        bool IsIndividual);
}
