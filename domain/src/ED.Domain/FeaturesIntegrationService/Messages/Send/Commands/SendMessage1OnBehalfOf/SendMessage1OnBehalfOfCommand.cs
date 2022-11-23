using MediatR;

namespace ED.Domain
{
    public record SendMessage1OnBehalfOfCommand(
        string SenderIdentifier,
        string? SenderPhone,
        string? SenderEmail,
        string? SenderFirstName,
        string? SenderLastName,
        int SenderTargetGroupId,
        string RecipientIdentifier,
        int RecipientTargetGroupId,
        string MessageSubject,
        string MessageBody,
        SendMessage1OnBehalfOfCommandBlob[] Blobs,
        string? ServiceOid,
        int SentViaLoginId,
        int? SentViaOperatorLoginId,
        string SendEvent)
        : IRequest<SendMessage1OnBehalfOfCommandResult>;

    public record SendMessage1OnBehalfOfCommandBlob(
        string FileName,
        string HashAlgorithm,
        string Hash,
        ulong Size,
        int BlobId);

    public record SendMessage1OnBehalfOfCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
