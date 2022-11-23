using System;

namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        public record GetForwardedMessageAsRecipientVO(
            int MessageId,
            DateTime DateSent,
            GetForwardedMessageAsRecipientVOProfile Sender,
            int TemplateId,
            string Subject,
            string? Rnu,
            string Body,
            string TemplateName,
            GetForwardedMessageAsRecipientVOBlob[] Blobs);

        public record GetForwardedMessageAsRecipientVOProfile(
            int ProfileId,
            string Name,
            ProfileType Type,
            bool IsReadOnly,
            string LoginName);

        public record GetForwardedMessageAsRecipientVOBlob(
            int BlobId,
            string FileName,
            string Hash,
            long? Size,
            string? DocumentRegistrationNumber,
            MalwareScanResultStatus Status,
            bool? IsMalicious,
            GetForwardedMessageAsRecipientVOBlobSignature[] Signatures);

        public record GetForwardedMessageAsRecipientVOBlobSignature(
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
