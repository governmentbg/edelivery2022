using MediatR;

namespace ED.Domain
{
    public record SendMessageInReplyToRequestCommand(
        string MessageSubject,
        string MessageBody,
        SendMessageInReplyToRequestCommandDocument[] Documents,
        int ReplyToMessageId,
        string? ServiceOid,
        int SenderProfileId,
        int SenderLoginId,
        string SendEvent)
        : IRequest<SendMessageInReplyToRequestCommandResult>;

    public record SendMessageInReplyToRequestCommandDocument(
        string FileName,
        string? DocumentRegistrationNumber,
        byte[] FileContent);

    public record SendMessageInReplyToRequestCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
