using System;
using Microsoft.EntityFrameworkCore;

namespace ED.Domain
{
    public partial interface ITicketsOpenQueryRepository
    {
        public record GetAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            DateTime? DateReceived,
            GetAsRecipientVOTicket Ticket,
            GetAsRecipientVOProfile Sender,
            GetAsRecipientVOProfile Recipient,
            string Subject,
            byte[] Body,
            int RecipientProfileKeyId,
            byte[] RecipientEncryptedKey,
            byte[] IV,
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

        [Keyless]
        public record MessageAsRecipientQO(
            int MessageId,
            DateTime DateSent,
            DateTime? DateReceived,
            int SenderProfileId,
            string SenderProfileName,
            ProfileType SenderProfileType,
            bool SenderProfileIsReadOnly,
            string SenderLoginName,
            int RecipientProfileId,
            string RecipientProfileName,
            ProfileType RecipientProfileType,
            bool RecipientProfileIsReadOnly,
            string RecipientLoginName,
            string Subject,
            byte[] Body,
            int ProfileKeyId,
            byte[] EncryptedKey,
            byte[] IV,
            string Type,
            string? DocumentIdentifier,
            TicketStatusStatus Status,
            DateTime? ServeDate,
            DateTime? AnnulDate,
            string? AnnulmentReason);
    }
}
