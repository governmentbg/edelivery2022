using System;
using System.Threading;
using System.Threading.Tasks;
using ED.Blobs;
using Google.Protobuf;
using Grpc.Core;
using static ED.Blobs.Blobs;

namespace ED.IntegrationService
{
    class BlobsServiceClient
    {
        // used for uploading tempory files for on behalf of actions
        public const int SystemProfileId = 1;

        // use the maxArrayLength of ArrayPool<byte>.Shared as we use it in the BlobReader
        private const int UploadChunkSize = 1 * 1024 * 1024; // 1 MB
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

        public class UploadBlobVO
        {
            public int? BlobId { get; set; }
            public bool IsMalicious { get; set; }
            public string Hash { get; set; }
            public string HashAlgorithm { get; set; }
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

        public async Task<UploadBlobVO> UploadProfileBlobAsync(
            string fileName,
            ReadOnlyMemory<byte> content,
            int profileId,
            int loginId,
            ProfileBlobAccessKeyType type,
            string documentRegistrationNumber,
            CancellationToken ct)
        {
            using (var call = this.client.UploadProfileBlob(cancellationToken: ct))
            {
                await call.RequestStream.WriteAsync(
                    new UploadProfileBlobRequest
                    {
                        Header = new UploadProfileBlobRequest.Types.BlobUploadHeader
                        {
                            FileName = fileName,
                            Size = content.Length,
                            ProfileId = profileId,
                            LoginId = loginId,
                            Type = MapProfileBlobAccessKeyType(type),
                            DocumentRegistrationNumber = documentRegistrationNumber,
                        }
                    });

                int start = 0;
                while (content.Length - start > 0)
                {
                    int end = Math.Min(content.Length, start + UploadChunkSize);
                    await call.RequestStream.WriteAsync(
                        new UploadProfileBlobRequest
                        {
                            Chunk = new BlobChunk
                            {
                                // check this PR for explanation
                                // https://github.com/protocolbuffers/protobuf/pull/7645
                                Data = UnsafeByteOperations.UnsafeWrap(content.Slice(start, end - start))
                            }
                        });
                    start = end;
                }

                await call.RequestStream.CompleteAsync();

                var resp = await call;
                return new UploadBlobVO
                {
                    BlobId = resp.BlobId,
                    IsMalicious = resp.IsMalicious,
                    Hash = resp.Hash,
                    HashAlgorithm = resp.HashAlgorithm
                };
            }
        }

        private static ED.Blobs.ProfileBlobAccessKeyType MapProfileBlobAccessKeyType(
            ProfileBlobAccessKeyType type)
        {
            switch (type)
            {
                case ProfileBlobAccessKeyType.Temporary:
                    return ED.Blobs.ProfileBlobAccessKeyType.Temporary;
                case ProfileBlobAccessKeyType.Storage:
                case ProfileBlobAccessKeyType.Registration:
                case ProfileBlobAccessKeyType.Template:
                case ProfileBlobAccessKeyType.PdfStamp:
                    throw new Exception("Unsupported ProfileBlobAccessKeyType");
                default:
                    throw new Exception("Uknown ProfileBlobAccessKeyType");
            }
        }
    }
}
