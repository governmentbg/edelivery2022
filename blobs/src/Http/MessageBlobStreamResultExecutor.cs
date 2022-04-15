using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ED.Blobs
{
    public class MessageBlobStreamResultExecutor
        : BlobStreamResultExecutor<MessageBlobStreamResult>
    {
        private readonly BlobReader blobReader;

        public MessageBlobStreamResultExecutor(
            BlobReader blobReader,
            ILoggerFactory loggerFactory)
            : base(blobReader, loggerFactory)
        {
            this.blobReader = blobReader;
        }

        protected override async Task CopyBlobContentToStreamAsync(
            MessageBlobStreamResult result,
            Stream destination,
            int bufferSize,
            CancellationToken ct)
        {
            await this.blobReader.CopyMessageBlobContentToStreamAsync(
                result.ProfileId,
                result.MessageId,
                result.BlobId,
                destination,
                bufferSize,
                ct);
        }

        protected override async Task CopyBlobContentToStreamAsync(
            MessageBlobStreamResult result,
            Stream destination,
            int bufferSize,
            long offset,
            long length,
            CancellationToken ct)
        {
            await this.blobReader.CopyMessageBlobContentToStreamAsync(
                result.ProfileId,
                result.MessageId,
                result.BlobId,
                destination,
                bufferSize,
                offset,
                length,
                ct);
        }
    }
}
