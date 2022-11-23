using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentDocumentContentVO(
            int BlobId,
            string FileName,
            int MessageId,
            byte[]? Timestamp,
            string? DocumentRegistrationNumber,
            GetSentDocumentContentVOSignature[] Signatures);

        public record GetSentDocumentContentVOSignature(
            byte[] SigningCertificate,
            bool CoversDocument,
            bool CoversPriorRevision,
            bool IsTimestamp,
            DateTime SignDate,
            bool ValidAtTimeOfSigning,
            string Issuer,
            string Subject,
            string SerialNumber,
            int Version,
            DateTime ValidFrom,
            DateTime ValidTo);
    }
}
