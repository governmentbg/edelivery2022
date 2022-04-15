using System;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            DateTime? DateReceived,
            GetAsRecipientVOProfile Sender,
            GetAsRecipientVOProfile Recipient,
            int TemplateId,
            string Subject,
            string? Orn,
            string? ReferencedOrn,
            string? AdditionalIdentifier,
            byte[] Body,
            ForwardStatus ForwardStatusId,
            int RecipientProfileKeyId,
            byte[] RecipientEncryptedKey,
            byte[] IV,
            string TemplateName,
            GetAsRecipientVOBlob[] Blobs,
            int? ForwardedMessageId);

        public record GetAsRecipientVOProfile(
            int ProfileId,
            string Name,
            ProfileType Type,
            bool IsReadOnly,
            string LoginName);

        public record GetAsRecipientVOBlob(
            int BlobId,
            string FileName,
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
