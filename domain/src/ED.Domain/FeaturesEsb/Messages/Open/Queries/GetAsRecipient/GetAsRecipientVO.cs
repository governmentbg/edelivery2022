using System;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenQueryRepository
    {
        public record GetAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            GetAsRecipientVOProfileSender Sender,
            DateTime? DateReceived,
            GetAsRecipientVORecipientLogin? RecipientLogin,
            string Subject,
            string? Orn,
            string? ReferencedOrn,
            string? AdditionalIdentifier,
            int TemplateId,
            byte[] Body,
            int ProfileKeyId,
            byte[] EncryptedKey,
            byte[] IV,
            GetAsRecipientVOBlob[] Blobs,
            int? ForwardedMessageId);

        public record GetAsRecipientVOProfileSender(
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
