using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ED.Blobs;
using Google.Protobuf;
using Grpc.Core;
using static ED.Blobs.Blobs;
using static ED.Blobs.UploadProfileBlobRequest.Types;

namespace ED.Domain
{
    class BlobsServiceClient
    {
        // use the maxArrayLength of ArrayPool<byte>.Shared as we use it in the BlobReader
        private const int UploadChunkSize = 1 * 1024 * 1024; // 1 MB

        private readonly BlobsClient client;
        public BlobsServiceClient(BlobsClient client)
        {
            this.client = client;
        }

        public record UploadBlobVO(
            int? BlobId,
            bool IsMalicious,
            string? Hash,
            string? HashAlgorithm);

        public record DownloadBlobToArrayVO(
           string FileName,
           long Size,
           byte[] Content);

        public record DownloadBlobToStreamVO(
           string FileName,
           long Size);

        public async Task<UploadBlobVO> UploadSystemBlobAsync(
            string fileName,
            ReadOnlyMemory<byte> content,
            int? createdByLoginId,
            ProfileBlobAccessKeyType type,
            CancellationToken ct)
        {
            return await this.UploadProfileBlobAsync(
                fileName,
                content,
                Profile.SystemProfileId,
                createdByLoginId ?? Login.SystemLoginId,
                type,
                null,
                ct);
        }

        public async Task<DownloadBlobToStreamVO> DownloadSystemBlobToStreamAsync(
            int blobId,
            Stream outputStream,
            CancellationToken ct)
        {
            return await this.DownloadProfileBlobToStreamAsync(
                blobId,
                Profile.SystemProfileId,
                outputStream,
                ct);
        }

        public async Task<DownloadBlobToArrayVO> DownloadSystemBlobToArrayAsync(
            int blobId,
            CancellationToken ct)
        {
            return await this.DownloadProfileBlobToArrayAsync(
                blobId,
                Profile.SystemProfileId,
                ct);
        }

        public async Task<UploadBlobVO> UploadProfileBlobAsync(
            string fileName,
            ReadOnlyMemory<byte> content,
            int profileId,
            int loginId,
            ProfileBlobAccessKeyType type,
            string? documentRegistrationNumber,
            CancellationToken ct)
        {
            using var call = this.client.UploadProfileBlob(cancellationToken: ct);
            await call.RequestStream.WriteAsync(
                new UploadProfileBlobRequest
                {
                    Header = new BlobUploadHeader
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

            return new UploadBlobVO(
                resp.BlobId,
                resp.IsMalicious,
                resp.Hash,
                resp.HashAlgorithm);
        }

        public async Task<DownloadBlobToArrayVO> DownloadProfileBlobToArrayAsync(
            int blobId,
            int profileId,
            CancellationToken ct)
        {
            using var call = this.client.DownloadProfileBlob(
                new DownloadProfileBlobRequest
                {
                    BlobId = blobId,
                    ProfileId = profileId,
                },
                cancellationToken: ct);

            if (!await call.ResponseStream.MoveNext() ||
                call.ResponseStream.Current.Header == null)
            {
                throw new Exception("The stream should begin with a header");
            }

            var header = call.ResponseStream.Current.Header;

            int position = 0;
            byte[] content = new byte[header.Size];

            await foreach (var chunkResp in call.ResponseStream.ReadAllAsync(ct))
            {
                if (chunkResp.Chunk == null)
                {
                    throw new Exception("The rest of the stream should be only chunks");
                }

                int chunkLength = chunkResp.Chunk.Data.Length;

                chunkResp.Chunk.Data.Span.CopyTo(content.AsSpan(position, chunkLength));

                position += chunkLength;
            }

            return new DownloadBlobToArrayVO(
                header.FileName,
                header.Size,
                content);
        }

        public async Task<DownloadBlobToArrayVO> DownloadMessageBlobToArrayAsync(
            int profileId,
            int blobId,
            int messageId,
            CancellationToken ct)
        {
            using var call = this.client.DownloadMessageBlob(
                new DownloadMessageBlobRequest
                {
                    ProfileId = profileId,
                    MessageId = messageId,
                    BlobId = blobId,
                },
                cancellationToken: ct);

            if (!await call.ResponseStream.MoveNext() ||
                call.ResponseStream.Current.Header == null)
            {
                throw new Exception("The stream should begin with a header");
            }

            var header = call.ResponseStream.Current.Header;

            int position = 0;
            byte[] content = new byte[header.Size];

            await foreach (var chunkResp in call.ResponseStream.ReadAllAsync(ct))
            {
                if (chunkResp.Chunk == null)
                {
                    throw new Exception("The rest of the stream should be only chunks");
                }

                int chunkLength = chunkResp.Chunk.Data.Length;

                chunkResp.Chunk.Data.Span.CopyTo(content.AsSpan(position, chunkLength));

                position += chunkLength;
            }

            return new DownloadBlobToArrayVO(
                header.FileName,
                header.Size,
                content);
        }

        private async Task<DownloadBlobToStreamVO> DownloadProfileBlobToStreamAsync(
            int blobId,
            int profileId,
            Stream outputStream,
            CancellationToken ct)
        {
            using var call = this.client.DownloadProfileBlob(
                new DownloadProfileBlobRequest
                {
                    BlobId = blobId,
                    ProfileId = profileId,
                },
                cancellationToken: ct);

            if (!await call.ResponseStream.MoveNext() ||
                call.ResponseStream.Current.Header == null)
            {
                throw new Exception("The stream should begin with a header");
            }

            var header = call.ResponseStream.Current.Header;

            await foreach (var chunkResp in call.ResponseStream.ReadAllAsync(ct))
            {
                if (chunkResp.Chunk == null)
                {
                    throw new Exception("The rest of the stream should be only chunks");
                }

                outputStream.Write(chunkResp.Chunk.Data.Span);
            }

            return new DownloadBlobToStreamVO(
                header.FileName,
                header.Size);
        }

        private static ED.Blobs.ProfileBlobAccessKeyType MapProfileBlobAccessKeyType(
            ProfileBlobAccessKeyType type)
            => type switch
            {
                ProfileBlobAccessKeyType.Temporary => ED.Blobs.ProfileBlobAccessKeyType.Temporary,
                ProfileBlobAccessKeyType.Storage => ED.Blobs.ProfileBlobAccessKeyType.Storage,
                ProfileBlobAccessKeyType.Registration => ED.Blobs.ProfileBlobAccessKeyType.Registration,
                ProfileBlobAccessKeyType.Template => ED.Blobs.ProfileBlobAccessKeyType.Template,
                ProfileBlobAccessKeyType.PdfStamp => ED.Blobs.ProfileBlobAccessKeyType.PdfStamp,
                _ => throw new Exception("Uknown ProfileBlobAccessKeyType"),
            };
    }
}
