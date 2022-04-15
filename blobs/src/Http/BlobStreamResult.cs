using Microsoft.AspNetCore.Mvc;

namespace ED.Blobs
{
    public abstract class BlobStreamResult : ActionResult
    {
        protected BlobStreamResult(int blobId)
        {
            this.BlobId = blobId;
        }

        public int BlobId { get; private init; }
    }
}
