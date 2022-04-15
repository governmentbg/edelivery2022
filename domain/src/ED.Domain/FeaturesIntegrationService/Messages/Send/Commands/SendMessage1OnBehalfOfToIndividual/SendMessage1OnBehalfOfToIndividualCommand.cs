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
        SendMessage1OnBehalfOfToIndividualCommandDocument[] Documents,
        string? ServiceOid,
        int OnBehalfOfProfileId,
        int OnBehalfOfLoginId,
        int? OnBehalfOfOperatorLoginId,
        string SendEvent)
        : IRequest<SendMessage1OnBehalfOfToIndividualCommandResult>;

    public record SendMessage1OnBehalfOfToIndividualCommandDocument(
        string FileName,
        string? DocumentRegistrationNumber,
        byte[] FileContent);

    public record SendMessage1OnBehalfOfToIndividualCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
