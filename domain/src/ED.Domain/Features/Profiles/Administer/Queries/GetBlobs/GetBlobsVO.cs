using System;

namespace ED.Domain
{
    public partial interface IProfileAdministerQueryRepository
    {
        public record GetBlobsVO(
            int BlobId,
            string FileName,
            string? Description,
            DateTime CreateDate,
            string CreatedBy);
    }
}
