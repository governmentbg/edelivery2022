using MediatR;

namespace ED.Domain
{
    public record SendMessage1OnBehalfOfToLegalEntityCommand(
        string SenderIdentifier,
        string RecipientIdentifier,
        string MessageSubject,
        string MessageBody,
        SendMessage1OnBehalfOfToLegalEntityCommandBlob[] Blobs,
        string? ServiceOid,
        int SentViaLoginId,
        int? SentViaOperatorLoginId,
        string SendEvent)
        : IRequest<SendMessage1OnBehalfOfToLegalEntityCommandResult>;

    public record SendMessage1OnBehalfOfToLegalEntityCommandBlob(
        string FileName,
        string HashAlgorithm,
        string Hash,
        ulong Size,
        int BlobId);

    public record SendMessage1OnBehalfOfToLegalEntityCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
