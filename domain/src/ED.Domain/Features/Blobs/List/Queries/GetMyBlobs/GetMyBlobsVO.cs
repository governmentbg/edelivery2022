using System;

namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        public record GetMyBlobsVO(
            int BlobId,
            string FileName,
            string HashAlgorithm,
            string Hash,
            long Size,
            DateTime CreateDate,
            bool IsNotMalicious,
            bool IsMalicious,
            bool IsNotSure);
    }
}
