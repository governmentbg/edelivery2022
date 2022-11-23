using System;

namespace ED.Domain
{
    public partial interface IIntegrationServiceMessagesOpenQueryRepository
    {
        public record GetSentDocumentsContentVO(
            int BlobId,
            string FileName,
            byte[]? Timestamp,
            string? DocumentRegistrationNumber,
            GetSentDocumentsContentVOSignature[] Signatures);

        public record GetSentDocumentsContentVOSignature(
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
