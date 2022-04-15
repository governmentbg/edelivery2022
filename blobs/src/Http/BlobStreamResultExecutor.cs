using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace ED.Blobs
{
    public abstract class BlobStreamResultExecutor<T>
        : FileResultExecutorBase, IActionResultExecutor<T>
        where T : BlobStreamResult
    {
        class DummyFileResult : FileResult
        {
            public DummyFileResult(string contentType)
                : base(contentType)
            {
            }
        }

        private FileExtensionContentTypeProvider contentTypeProvider = new();

        private readonly BlobReader blobReader;

        protected BlobStreamResultExecutor(
            BlobReader blobReader,
            ILoggerFactory loggerFactory)
            : base(CreateLogger<BlobStreamResultExecutor<T>>(loggerFactory))
        {
            this.blobReader = blobReader;
        }

        public async Task ExecuteAsync(
            ActionContext context,
            T result)
        {
            context = context ?? throw new ArgumentNullException(nameof(context));
            result = result ?? throw new ArgumentNullException(nameof(result));

            try
            {
                var ct = context.HttpContext.RequestAborted;

                var blobInfo = await this.blobReader.GetBlobInfoAsync(
                    result.BlobId,
                    ct);

                if (blobInfo == null)
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var fileResult =
                    new DummyFileResult(this.GetFileContentType(blobInfo.Filename))
                    {
                        FileDownloadName = blobInfo.Filename,
                        LastModified = null,
                        EntityTag = new EntityTagHeaderValue(
                            $"\"{Convert.ToBase64String(blobInfo.Version)}\""),
                        EnableRangeProcessing = true,
                    };

                // using the FileResultExecutorBase for the range negotiation
                var (range, rangeLength, serveBody) = this.SetHeadersAndLog(
                    context,
                    fileResult,
                    blobInfo.Size,
                    fileResult.EnableRangeProcessing,
                    fileResult.LastModified,
                    fileResult.EntityTag);

                if (!serveBody)
                {
                    return;
                }

                if (range != null && range.From != null)
                {
                    if (rangeLength == 0)
                    {
                        return;
                    }

                    await this.CopyBlobContentToStreamAsync(
                        result,
                        context.HttpContext.Response.Body,
                        FileResultExecutorBase.BufferSize,
                        range.From.Value,
                        rangeLength,
                        ct);
                }
                else
                {
                    await this.CopyBlobContentToStreamAsync(
                        result,
                        context.HttpContext.Response.Body,
                        FileResultExecutorBase.BufferSize,
                        ct);
                }
            }
            catch (OperationCanceledException)
            {
                this.Logger.LogInformation("Download cancelled");
                // Don't throw this exception, it's most likely caused by
                // the client disconnecting.
                // However, if it was cancelled for any other reason we need
                // to prevent empty responses.
                context.HttpContext.Abort();
            }
        }

        protected abstract Task CopyBlobContentToStreamAsync(
            T result,
            Stream destination,
            int bufferSize,
            CancellationToken ct);

        protected abstract Task CopyBlobContentToStreamAsync(
            T result,
            Stream destination,
            int bufferSize,
            long offset,
            long length,
            CancellationToken ct);

        private string GetFileContentType(string fileName)
        {
            if (!this.contentTypeProvider.TryGetContentType(
                fileName,
                out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }
    }
}
