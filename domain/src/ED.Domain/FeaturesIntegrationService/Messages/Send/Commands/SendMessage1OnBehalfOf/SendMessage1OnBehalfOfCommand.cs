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
        SendMessage1OnBehalfOfCommandDocument[] Documents,
        string? ServiceOid,
        int OnBehalfOfProfileId,
        int OnBehalfOfLoginId,
        int? OnBehalfOfOperatorLoginId,
        string SendEvent)
        : IRequest<SendMessage1OnBehalfOfCommandResult>;

    public record SendMessage1OnBehalfOfCommandDocument(
        string FileName,
        string? DocumentRegistrationNumber,
        byte[] FileContent);

    public record SendMessage1OnBehalfOfCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
