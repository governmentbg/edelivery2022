using MediatR;

namespace ED.Domain
{
    public record SendMessage1Command(
        string RecipientIdentifier,
        string? RecipientPhone,
        string? RecipientEmail,
        int RecipientTargetGroupId,
        string MessageSubject,
        string MessageBody,
        SendMessage1CommandBlob[] Blobs,
        string? ServiceOid,
        int SenderProfileId,
        int SenderLoginId,
        string SendEvent)
        : IRequest<SendMessage1CommandResult>;

    public record SendMessage1CommandBlob(
        string FileName,
        string HashAlgorithm,
        string Hash,
        ulong Size,
        int BlobId);

    public record SendMessage1CommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
