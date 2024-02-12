using MediatR;

namespace ED.Domain
{
    public record JobsUpdateMessageTranslationRequestCommand(
        int MessageTranslationId,
        int? SourceBlobId,
        long? RequestId,
        MessageTranslationRequestStatus Status,
        string? ErrorMessage)
        : IRequest;
}
