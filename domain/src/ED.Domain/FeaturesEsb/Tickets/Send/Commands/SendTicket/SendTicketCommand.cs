using System;
using MediatR;

namespace ED.Domain
{
    public record SendTicketCommand(
        bool IsRecipientIndividual,
        int RecipientProfileId,
        string RecipientIdentifier,
        int SenderProfileId,
        int SenderLoginId,
        int TemplateId,
        string Subject,
        string Body,
        TicketType Type,
        string? DocumentSeries,
        string DocumentNumber,
        DateTime IssueDate,
        string VehicleNumber,
        DateTime ViolationDate,
        string ViolatedProvision,
        string PenaltyProvision,
        string DueAmount,
        string DiscountedPaymentAmount,
        string IBAN,
        string BIC,
        string PaymentReason,
        string? NotificationEmail,
        string? NotificationPhone,
        SendTicketCommandBlob Document,
        string? DocumentIdentifier)
        : IRequest<int>;

    public record SendTicketCommandBlob(
        string FileName,
        string HashAlgorithm,
        string Hash,
        ulong Size,
        int BlobId);
}
