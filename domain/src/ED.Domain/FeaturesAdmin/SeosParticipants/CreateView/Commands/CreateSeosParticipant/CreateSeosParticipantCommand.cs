using MediatR;

namespace ED.Domain
{
    public record CreateSeosParticipantCommand(
        string Identifier,
        string AS4Node
    ) : IRequest;
}
