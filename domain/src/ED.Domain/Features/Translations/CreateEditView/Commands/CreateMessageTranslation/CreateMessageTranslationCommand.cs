using MediatR;

namespace ED.Domain
{
    public record CreateMessageTranslationCommand(
        int MessageId,
        int ProfileId,
        string SourceLanguage,
        string TargetLanguage,
        int LoginId)
        : IRequest;
}
