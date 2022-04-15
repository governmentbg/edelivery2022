using MediatR;

namespace ED.Domain
{
    public record SendMessage1OnBehalfOfToLegalEntityCommand(
        string SenderIdentifier,
        string RecipientIdentifier,
        string MessageSubject,
        string MessageBody,
        SendMessage1OnBehalfOfToLegalEntityCommandDocument[] Documents,
        string? ServiceOid,
        int OnBehalfOfProfileId,
        int OnBehalfOfLoginId,
        int? OnBehalfOfOperatorLoginId,
        string SendEvent)
        : IRequest<SendMessage1OnBehalfOfToLegalEntityCommandResult>;

    public record SendMessage1OnBehalfOfToLegalEntityCommandDocument(
        string FileName,
        string? DocumentRegistrationNumber,
        byte[] FileContent);

    public record SendMessage1OnBehalfOfToLegalEntityCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
