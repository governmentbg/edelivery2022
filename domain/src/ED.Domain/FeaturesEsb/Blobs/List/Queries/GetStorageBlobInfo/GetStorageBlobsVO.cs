using System;

namespace ED.Domain
{
    public partial interface IEsbBlobsListQueryRepository
    {
        public record GetStorageBlobInfoVO(
            int BlobId,
            string FileName,
            long? Size,
            string? DocumentRegistrationNumber,
            bool? IsMalicious,
            string? Hash,
            string? HashAlgorithm,
            GetStorageBlobInfoVOSignature[] Signatures);

        public record GetStorageBlobInfoVOSignature(
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
