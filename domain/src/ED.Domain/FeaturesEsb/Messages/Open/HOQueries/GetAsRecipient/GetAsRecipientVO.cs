using System;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenHORepository
    {
        public record GetAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            GetAsRecipientVOProfileSender Sender,
            DateTime? DateReceived,
            GetAsRecipientVOProfileRecipient Recipient,
            GetAsRecipientVORecipientLogin? RecipientLogin,
            string Subject,
            string? Rnu,
            int TemplateId,
            string Body,
            GetAsRecipientVOBlob[] Blobs,
            int? ForwardedMessageId);

        public record GetAsRecipientVOProfileSender(
            int ProfileId,
            string Name);

        public record GetAsRecipientVOProfileRecipient(
            int ProfileId,
            string Name);

        public record GetAsRecipientVORecipientLogin(
            int LoginId,
            string Name);

        public record GetAsRecipientVOBlob(
            int BlobId,
            string FileName,
            long? Size,
            string? DocumentRegistrationNumber,
            bool? IsMalicious,
            string? Hash,
            string? HashAlgorithm,
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
