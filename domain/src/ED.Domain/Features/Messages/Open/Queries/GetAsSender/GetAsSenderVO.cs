using System;

namespace ED.Domain
{
    public partial interface IMessageOpenQueryRepository
    {
        public record GetAsSenderVO(
            int MessageId,
            DateTime DateSent,
            GetAsSenderVOProfile Sender,
            string Recipients,
            int TemplateId,
            string Subject,
            string? Rnu,
            byte[] Body,
            ForwardStatus ForwardStatusId,
            int ProfileKeyId,
            byte[] EncryptedKey,
            byte[] IV,
            string TemplateName,
            GetAsSenderVOBlob[] Blobs,
            int? ForwardedMessageId);

        public record GetAsSenderVOProfile(
            int ProfileId,
            string Name);

        public record GetAsSenderVOBlob(
            int BlobId,
            string FileName,
            string Hash,
            long? Size,
            string? DocumentRegistrationNumber,
            MalwareScanResultStatus Status,
            bool? IsMalicious,
            GetAsSenderVOBlobSignature[] Signatures);

        public record GetAsSenderVOBlobSignature(
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
