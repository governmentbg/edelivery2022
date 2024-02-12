using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using static ED.Blobs.BlobWriter;

namespace ED.Blobs
{
    [ApiController]
    public class EsbBlobsController
    {
        // Get the default form options so that we can use them to
        // set the default limits for request body data
        private static readonly FormOptions defaultFormOptions = new();

        public record BlobDO(
            string Name,
            long Size,
            string? HashAlgorithm,
            string? Hash,
            int? BlobId,
            MalwareScanStatus MalwareScanStatus,
            BlobSignatureStatus SignatureStatus,
            ErrorStatus ErrorStatus)
        {
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public MalwareScanStatus MalwareScanStatus { get; init; } =
                MalwareScanStatus;

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public BlobSignatureStatus SignatureStatus { get; init; } =
                SignatureStatus;

            [JsonConverter(typeof(JsonStringEnumConverter))]
            public ErrorStatus ErrorStatus { get; init; } =
                ErrorStatus;
        }

        [Authorize(Policy = Policies.EsbWriteProfileBlob)]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Route("api/v1/blobs")]
        public async Task<ActionResult<BlobDO>> PostProfileBlobAsync(
            [FromQuery] BlobWriter.ProfileBlobAccessKeyType type,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobReader blobReader,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            if (type != BlobWriter.ProfileBlobAccessKeyType.Temporary
                && type != BlobWriter.ProfileBlobAccessKeyType.Storage)
            {
                return new JsonResult($"Unsupported value '{type}' for {nameof(type)} parameter")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            int profileId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserProfileId();
            int loginId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserLoginId();

            bool hasEnoughStorageSpace = await this.IsProfileQuotaEnoughAsync(
                profileId,
                loggerFactory,
                blobReader,
                httpContextAccessor,
                ct);

            if (!hasEnoughStorageSpace)
            {
                return new BlobDO(
                    string.Empty,
                    0,
                    null,
                    null,
                    null,
                    MalwareScanStatus.None,
                    BlobSignatureStatus.None,
                    ErrorStatus.InsufficientStorageSpace);
            }

            return await this.PostProfileBlobInternalAsync(
                profileId,
                loginId,
                type,
                true,
                loggerFactory,
                blobWriter,
                httpContextAccessor,
                ct);
        }

        [Authorize(Policy = Policies.EsbWriteProfileBlobOnBehalfOf)]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Route("api/v1/obo/blobs")]
        public async Task<ActionResult<BlobDO>> PostProfileBlobOnBehalfOfAsync(
            [FromQuery] BlobWriter.ProfileBlobAccessKeyType type,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobReader blobReader,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            if (type != BlobWriter.ProfileBlobAccessKeyType.Temporary
                && type != BlobWriter.ProfileBlobAccessKeyType.Storage)
            {
                return new JsonResult($"Unsupported value '{type}' for {nameof(type)} parameter")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            int representedProfileId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserRepresentedProfileId()
                ?? throw new ArgumentException($"Invalid authentication header");
            int loginId = httpContextAccessor.HttpContext!.User.GetAuthenticatedUserLoginId();

            bool hasEnoughStorageSpace = await this.IsProfileQuotaEnoughAsync(
                representedProfileId,
                loggerFactory,
                blobReader,
                httpContextAccessor,
                ct);

            if (!hasEnoughStorageSpace)
            {
                return new BlobDO(
                    string.Empty,
                    0,
                    null,
                    null,
                    null,
                    MalwareScanStatus.None,
                    BlobSignatureStatus.None,
                    ErrorStatus.InsufficientStorageSpace);
            }

            return await this.PostProfileBlobInternalAsync(
                representedProfileId,
                loginId,
                type,
                true,
                loggerFactory,
                blobWriter,
                httpContextAccessor,
                ct);
        }

        private async Task<bool> IsProfileQuotaEnoughAsync(
            int profileId,
            ILoggerFactory loggerFactory,
            BlobReader blobReader,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            try
            {
                var httpContext = httpContextAccessor.HttpContext!;
                var request = httpContext.Request;
                // The content-length will always be just a bit larger than the
                // actual file size but it is a good approximation.
                // NOTE!! this will not work if we ever send more than one file
                // in the multipart upload. If the code below is changed to
                // accept more files than content-length's usage should be revisited.
                long upperSizeLimit =
                    request.ContentLength
                    ?? throw new Exception("Content-Length header is missing.");

                return await blobReader.IsProfileQuotaEnoughAsync(
                    profileId,
                    upperSizeLimit,
                    ct);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                loggerFactory.CreateLogger<BlobsController>().LogDebug("Can not check profile quota");
                throw;
            }
        }

        private async Task<ActionResult<BlobDO>> PostProfileBlobInternalAsync(
            int profileId,
            int loginId,
            BlobWriter.ProfileBlobAccessKeyType type,
            bool extractPdfSignatures,
            ILoggerFactory loggerFactory,
            BlobWriter blobWriter,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            var httpContext = httpContextAccessor.HttpContext!;
            var request = httpContext.Request;

            try
            {
                if (!MultipartRequestHelper.IsMultipartContentType(request.ContentType))
                {
                    return new JsonResult($"Expected a multipart request, but got {request.ContentType}")
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                // The content-length will always be just a bit larger than the
                // actual file size but it is a good approximation.
                // NOTE!! this will not work if we ever send more than one file
                // in the multipart upload. If the code below is changed to
                // accept more files than content-length's usage should be revisited.
                long upperSizeLimit =
                    request.ContentLength
                    ?? throw new Exception("Content-Length header is missing.");

                var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(request.ContentType),
                    defaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, httpContext.Request.Body);

                BlobInfo? blobInfo = null;
                string? fileName = null;
                var section = await reader.ReadNextSectionAsync(ct);
                while (section != null)
                {
                    if (ContentDispositionHeaderValue.TryParse(
                            section.ContentDisposition,
                            out var contentDisposition) &&
                        MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        string file;
                        if (!string.IsNullOrEmpty(contentDisposition.FileNameStar.Value))
                        {
                            file = contentDisposition.FileNameStar.Value;
                        }
                        else if (!string.IsNullOrEmpty(contentDisposition.FileName.Value))
                        {
                            file = contentDisposition.FileName.Value;
                        }
                        else
                        {
                            throw new Exception("At least one of FileName/FileNameStar should have a value");
                        }

                        fileName = Path.GetFileName(file);

                        blobInfo = await blobWriter.UploadProfileBlobAsync(
                            section.Body,
                            fileName,
                            upperSizeLimit,
                            extractPdfSignatures,
                            profileId,
                            loginId,
                            type,
                            null,
                            ct);

                        // we are looking for the first file and nothing else
                        break;
                    }

                    // Drains any remaining section body that has not been consumed
                    // and reads the headers for the next section.
                    section = await reader.ReadNextSectionAsync(ct);
                }

                if (blobInfo == null || fileName == null)
                {
                    return new JsonResult("Non file content disposition found")
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };
                }

                if (blobInfo.MalwareScanStatus == MalwareScanStatus.IsMalicious)
                {
                    return new BlobDO(
                        fileName,
                        blobInfo.Size,
                        null,
                        null,
                        null,
                        MalwareScanStatus.IsMalicious,
                        BlobSignatureStatus.None,
                        ErrorStatus.None);
                }

                return new BlobDO(
                    fileName,
                    blobInfo.Size,
                    blobInfo.HashAlgorithm,
                    blobInfo.Hash,
                    blobInfo.BlobId,
                    blobInfo.MalwareScanStatus,
                    blobInfo.SignatureStatus,
                    ErrorStatus.None);
            }
            catch (OperationCanceledException)
            {
                loggerFactory.CreateLogger<BlobsController>().LogDebug("Upload cancelled");
                throw;
            }
        }
    }
}
