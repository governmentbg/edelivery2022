using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentDocumentContentByRegNumVO(
            int BlobId,
            string FileName,
            byte[] Content,
            byte[]? Timestamp,
            string? DocumentRegistrationNumber,
            GetSentDocumentContentByRegNumVOSignature[] Signatures);

        public record GetSentDocumentContentByRegNumVOSignature(
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
