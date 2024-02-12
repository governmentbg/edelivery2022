using MediatR;

namespace ED.Domain
{
    public record ArchiveMessageTranslationCommand(
        int MessageTranslationId,
        int LoginId)
        : IRequest;
}
