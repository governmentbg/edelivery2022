using MediatR;

namespace ED.Domain
{
    public record SendMessage1OnBehalfOfToIndividualCommand(
        string SenderIdentifier,
        string RecipientIdentifier,
        string? RecipientPhone,
        string? RecipientEmail,
        string? RecipientFirstName,
        string? RecipientLastName,
        string MessageSubject,
        string MessageBody,
        SendMessage1OnBehalfOfToIndividualCommandBlob[] Blobs,
        string? ServiceOid,
        int SentViaLoginId,
        int? SentViaOperatorLoginId,
        string SendEvent)
        : IRequest<SendMessage1OnBehalfOfToIndividualCommandResult>;

    public record SendMessage1OnBehalfOfToIndividualCommandBlob(
        string FileName,
        string HashAlgorithm,
        string Hash,
        ulong Size,
        int BlobId);

    public record SendMessage1OnBehalfOfToIndividualCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
