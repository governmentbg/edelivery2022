using System;

namespace ED.Domain
{
    public partial interface IBlobListQueryRepository
    {
        public record GetOutboxBlobsVO(
            int BlobId,
            string FileName,
            long Size,
            DateTime CreateDate,
            bool IsNotMalicious,
            bool IsMalicious,
            bool IsNotSure,
            int MessageId,
            string? MessageSubject);
    }
}
