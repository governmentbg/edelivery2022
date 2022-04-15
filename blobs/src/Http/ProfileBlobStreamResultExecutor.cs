using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ED.Blobs
{
    public class ProfileBlobStreamResultExecutor
        : BlobStreamResultExecutor<ProfileBlobStreamResult>
    {
        private readonly BlobReader blobReader;

        public ProfileBlobStreamResultExecutor(
            BlobReader blobReader,
            ILoggerFactory loggerFactory)
            : base(blobReader, loggerFactory)
        {
            this.blobReader = blobReader;
        }

        protected override async Task CopyBlobContentToStreamAsync(
            ProfileBlobStreamResult result,
            Stream destination,
            int bufferSize,
            CancellationToken ct)
        {
            await this.blobReader.CopyProfileBlobContentToStreamAsync(
                result.ProfileId,
                result.BlobId,
                destination,
                bufferSize,
                ct);
        }

        protected override async Task CopyBlobContentToStreamAsync(
            ProfileBlobStreamResult result,
            Stream destination,
            int bufferSize,
            long offset,
            long length,
            CancellationToken ct)
        {
            await this.blobReader.CopyProfileBlobContentToStreamAsync(
                result.ProfileId,
                result.BlobId,
                destination,
                bufferSize,
                offset,
                length,
                ct);
        }
    }
}
