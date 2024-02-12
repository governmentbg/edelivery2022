using System;

namespace ED.Domain
{
    public partial interface ITicketsOpenHORepository
    {
        public record GetAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            DateTime? DateReceived,
            GetAsRecipientVOTicket Ticket,
            GetAsRecipientVOProfile Sender,
            GetAsRecipientVOProfile Recipient,
            string Subject,
            string Body,
            GetAsRecipientVOBlob Document,
            string SafeBase64Url);

        public record GetAsRecipientVOTicket(
            string Type,
            GetAsRecipientVOTicketStatus Status);

        public record GetAsRecipientVOTicketStatus(
            TicketStatusStatus Status,
            DateTime? ServeDate,
            DateTime? AnnulDate,
            string? AnnulmentReason);

        public record GetAsRecipientVOProfile(
            int ProfileId,
            string Name,
            ProfileType Type,
            bool IsReadOnly,
            string LoginName);

        public record GetAsRecipientVOBlob(
            int BlobId,
            string FileName,
            string Hash,
            long? Size,
            string? DocumentRegistrationNumber,
            MalwareScanResultStatus Status,
            bool? IsMalicious,
            GetAsRecipientVOBlobSignature[] Signatures);

        public record GetAsRecipientVOBlobSignature(
            bool CoversDocument,
            DateTime SignDate,
            bool IsTimestamp,
            bool ValidAtTimeOfSigning,
            string Issuer,
            string Subject,
            DateTime ValidFrom,
            DateTime ValidTo);
    }
}
