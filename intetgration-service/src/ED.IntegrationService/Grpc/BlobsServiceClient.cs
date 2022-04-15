using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Blobs;
using Grpc.Core;
using static ED.Blobs.Blobs;

namespace ED.IntegrationService
{
    class BlobsServiceClient
    {
        private readonly BlobsClient client;

        public BlobsServiceClient(BlobsClient client)
        {
            this.client = client;
        }

        public class DownloadBlobToArrayVO
        {
            public string FileName { get; set; }
            public long Size { get; set; }
            public byte[] Content { get; set; }
        }

        public async Task<DownloadBlobToArrayVO> DownloadMessageBlobToArrayAsync(
           int profileId,
           int blobId,
           int messageId,
           CancellationToken ct)
        {
            using (var call = this.client.DownloadMessageBlob(
                new DownloadMessageBlobRequest
                {
                    ProfileId = profileId,
                    MessageId = messageId,
                    BlobId = blobId,
                },
                cancellationToken: ct))
            {
                if (!await call.ResponseStream.MoveNext() ||
                    call.ResponseStream.Current.Header == null)
                {
                    throw new Exception("The stream should begin with a header");
                }

                var header = call.ResponseStream.Current.Header;

                int position = 0;
                byte[] content = new byte[header.Size];

                while (await call.ResponseStream.MoveNext(ct).ConfigureAwait(false))
                {
                    var chunkResp = call.ResponseStream.Current;

                    if (chunkResp.Chunk == null)
                    {
                        throw new Exception("The rest of the stream should be only chunks");
                    }

                    int chunkLength = chunkResp.Chunk.Data.Length;

                    chunkResp.Chunk.Data.Span.CopyTo(content.AsSpan(position, chunkLength));

                    position += chunkLength;
                }

                return new DownloadBlobToArrayVO
                {
                    FileName = header.FileName,
                    Size = header.Size,
                    Content = content,
                };
            }
        }
    }
}
