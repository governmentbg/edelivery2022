using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;

namespace ED.Blobs
{
    public class BlobsService : Blobs.BlobsBase
    {
        private readonly BlobWriter blobWriter;
        private readonly BlobReader blobReader;
        public BlobsService(BlobWriter blobWriter, BlobReader blobReader)
        {
            this.blobWriter = blobWriter;
            this.blobReader = blobReader;
        }

        public override async Task DownloadProfileBlob(
            DownloadProfileBlobRequest request,
            IServerStreamWriter<DownloadProfileBlobResponse> responseStream,
            ServerCallContext context)
        {
            int blobId = request.BlobId;
            CancellationToken ct = context.CancellationToken;

            var blobInfo = await this.blobReader.GetBlobInfoAsync(blobId, ct);

            if (blobInfo == null)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.NotFound,
                        $"Could not find blob with id '{blobId}'"));
            }

            await responseStream.WriteAsync(
                new DownloadProfileBlobResponse
                {
                    Header = new BlobDownloadHeader
                    {
                        FileName = blobInfo.Filename,
                        Size = blobInfo.Size,
                    }
                });

            await this.blobReader.CopyProfileBlobContentToStreamWriterAsync(
                blobId,
                responseStream,
                mem =>
                    new DownloadProfileBlobResponse
                    {
                        Chunk = new BlobChunk
                        {
                            // check this PR for explanation
                            // https://github.com/protocolbuffers/protobuf/pull/7645
                            Data = UnsafeByteOperations.UnsafeWrap(mem),
                        }
                    },
                request.ProfileId,
                ct
            );
        }

        public override async Task<UploadProfileBlobResponse> UploadProfileBlob(
            IAsyncStreamReader<UploadProfileBlobRequest> requestStream,
            ServerCallContext context)
        {
            var ct = context.CancellationToken;

            if (!await requestStream.MoveNext(ct) ||
                requestStream.Current.Header == null)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.FailedPrecondition,
                        "The stream should begin with a header"));
            }

            var header = requestStream.Current.Header;
            string fileName = header.FileName;
            long size = header.Size;
            bool extractPdfSignatures = header.ExtractPdfSignatures;
            int profileId = header.ProfileId;
            int loginId = header.LoginId;
            string? documentRegistrationNumber = header.DocumentRegistrationNumber;

            BlobWriter.ProfileBlobAccessKeyType type =
                header.Type switch
                {
                    ProfileBlobAccessKeyType.Temporary => BlobWriter.ProfileBlobAccessKeyType.Temporary,
                    ProfileBlobAccessKeyType.Storage => BlobWriter.ProfileBlobAccessKeyType.Storage,
                    ProfileBlobAccessKeyType.Registration => BlobWriter.ProfileBlobAccessKeyType.Registration,
                    ProfileBlobAccessKeyType.Template => BlobWriter.ProfileBlobAccessKeyType.Template,
                    ProfileBlobAccessKeyType.PdfStamp => BlobWriter.ProfileBlobAccessKeyType.PdfStamp,
                    ProfileBlobAccessKeyType.Translation => BlobWriter.ProfileBlobAccessKeyType.Translation,
                    _ => throw new Exception("Unknown ProfileBlobAccessKeyType"),
                };

            var blobInfo = await this.blobWriter.UploadProfileBlobFromStreamReaderAsync(
                requestStream,
                next =>
                {
                    if (next.Chunk == null)
                    {
                        throw new Exception("After the header there should be only chunks");
                    }

                    if (next.Chunk.Data.Length == 0)
                    {
                        throw new Exception("Zero size chunks are not allowed.");
                    }

                    return next.Chunk.Data.Memory;
                },
                fileName,
                size,
                extractPdfSignatures,
                profileId,
                loginId,
                type,
                documentRegistrationNumber,
                ct);

            return new UploadProfileBlobResponse
            {
                BlobId = blobInfo.BlobId,
                IsMalicious = blobInfo.MalwareScanStatus == MalwareScanStatus.IsMalicious,
                Hash = blobInfo.Hash,
                HashAlgorithm = blobInfo.HashAlgorithm,
            };
        }

        public override async Task DownloadMessageBlob(
            DownloadMessageBlobRequest request,
            IServerStreamWriter<DownloadMessageBlobResponse> responseStream,
            ServerCallContext context)
        {
            int blobId = request.BlobId;
            CancellationToken ct = context.CancellationToken;

            var blobInfo = await this.blobReader.GetBlobInfoAsync(blobId, ct);

            if (blobInfo == null)
            {
                throw new RpcException(
                    new Status(
                        StatusCode.NotFound,
                        $"Could not find blob with id '{blobId}'"));
            }

            await responseStream.WriteAsync(
                new DownloadMessageBlobResponse
                {
                    Header = new BlobDownloadHeader
                    {
                        FileName = blobInfo.Filename,
                        Size = blobInfo.Size,
                    }
                });

            await this.blobReader.CopyMessageBlobContentToStreamWriterAsync(
                request.ProfileId,
                request.MessageId,
                blobId,
                responseStream,
                mem =>
                    new DownloadMessageBlobResponse
                    {
                        Chunk = new BlobChunk
                        {
                            // check this PR for explanation
                            // https://github.com/protocolbuffers/protobuf/pull/7645
                            Data = UnsafeByteOperations.UnsafeWrap(mem),
                        }
                    },
                ct
            );
        }
    }
}
