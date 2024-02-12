using MediatR;

namespace ED.Domain
{
    public record JobsCloseMessageTranslationRequestsCommand(
        int MessageTranslationId)
        : IRequest;
}
