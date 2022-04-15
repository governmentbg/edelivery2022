using MediatR;

namespace ED.Domain
{
    public record DeleteSeosParticipantCommand(
        int ParticipantId
    ) : IRequest<Unit>;
}
