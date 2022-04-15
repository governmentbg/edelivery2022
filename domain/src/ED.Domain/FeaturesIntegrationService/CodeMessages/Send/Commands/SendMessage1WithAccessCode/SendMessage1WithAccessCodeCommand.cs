using MediatR;

namespace ED.Domain
{
    public record SendMessage1WithAccessCodeCommand(
        string RecipientFirstName,
        string RecipientMiddleName,
        string RecipientLastName,
        string RecipientIdentifier,
        string RecipientEmail,
        string RecipientPhone,
        string MessageSubject,
        string MessageBody,
        SendMessage1WithAccessCodeCommandDocument[] Documents,
        string? ServiceOid,
        int SenderProfileId,
        int SenderLoginId,
        string SendEvent)
        : IRequest<SendMessage1WithAccessCodeCommandResult>;

    public record SendMessage1WithAccessCodeCommandDocument(
        string FileName,
        string? DocumentRegistrationNumber,
        byte[] FileContent);

    public record SendMessage1WithAccessCodeCommandResult(
        bool IsSuccessful,
        string Error,
        int? MessageId);
}
