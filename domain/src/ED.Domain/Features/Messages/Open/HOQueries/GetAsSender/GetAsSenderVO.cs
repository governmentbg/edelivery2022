using System;

namespace ED.Domain
{
    public partial interface IMessageOpenHORepository
    {
        public record GetAsSenderVO(
            int MessageId,
            DateTime DateSent,
            GetVOProfile Sender,
            string Recipients,
            int TemplateId,
            string Subject,
            string? Orn,
            string? ReferencedOrn,
            string? AdditionalIdentifier,
            string Body,
            ForwardStatus ForwardStatusId,
            string TemplateName,
            GetAsSenderVOBlob[] Blobs,
            int? ForwardedMessageId);

        public record GetVOProfile(
            int ProfileId,
            string Name);

        public record GetAsSenderVOBlob(
            int BlobId,
            string FileName,
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
