using System;

namespace ED.Domain
{
    public partial interface IEsbMessagesOpenHORepository
    {
        public record GetAsSenderVO(
            int MessageId,
            DateTime DateSent,
            GetAsSenderVOProfile[] Recipients,
            string Subject,
            string? Rnu,
            int TemplateId,
            string Body,
            GetAsSenderVOBlob[] Blobs,
            int? ForwardedMessageId);

        public record GetAsSenderVOProfile(
            int ProfileId,
            string Name,
            DateTime? DateReceived);

        public record GetAsSenderVOBlob(
            int BlobId,
            string FileName,
            long? Size,
            string? DocumentRegistrationNumber,
            bool? IsMalicious,
            string? Hash,
            string? HashAlgorithm,
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
