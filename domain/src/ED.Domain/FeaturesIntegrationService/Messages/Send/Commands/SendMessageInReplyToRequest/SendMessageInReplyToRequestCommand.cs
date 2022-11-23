using MediatR;

namespace ED.Domain
{
    public record SendMessageInReplyToRequestCommand(
        string MessageSubject,
        string MessageBody,
        SendMessageInReplyToRequestCommandBlob[] Blobs,
        int ReplyToMessageId,
        string? ServiceOid,
        int SenderProfileId,
        int SenderLoginId,
        string SendEvent)
        : IRequest<SendMessageInReplyToRequestCommandResult>;

    public record SendMessageInReplyToRequestCommandBlob(
        string FileName,
        string HashAlgorithm,
        string Hash,
        ulong Size,
        int BlobId);

    public record SendMessageInReplyToRequestCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
