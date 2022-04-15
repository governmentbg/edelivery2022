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
    public class BlobsController
    {
        // Get the default form options so that we can use them to
        // set the default limits for request body data
        private static readonly FormOptions defaultFormOptions = new();

        [AllowAnonymous]
        [HttpGet]
        [Route("profile")]
        public ActionResult GetProfileBlob(
            [FromQuery(Name = "t")] string token,
            [FromServices] BlobTokenParser blobTokenParser)
        {
            var parsedToken = blobTokenParser.ParseProfileBlobToken(token);

            if (parsedToken == null)
            {
                return new ContentResult()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain;charset=UTF-8",
                    Content = "Линкът за сваляне е изтекъл или невалиден. Моля, презаредете страницата на хранилището и опитайте отново.",
                };
            }

            (int profileId, int blobId) = parsedToken;

            return new ProfileBlobStreamResult(profileId, blobId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("message")]
        public ActionResult GetMessageBlob(
            [FromQuery(Name = "t")] string token,
            [FromServices] BlobTokenParser blobTokenParser)
        {
            var parsedToken = blobTokenParser.ParseMessageBlobToken(token);

            if (parsedToken == null)
            {
                return new ContentResult()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain;charset=UTF-8",
                    Content = "Линкът за сваляне е изтекъл или невалиден. Моля, презаредете страницата на съобщението и опитайте отново.",
                };
            }

            (int profileId, int messageId, int blobId) = parsedToken;
            return new MessageBlobStreamResult(profileId, messageId, blobId);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("system")]
        public ActionResult GetSystemBlob(
            [FromQuery(Name = "t")] string token,
            [FromServices] BlobTokenParser blobTokenParser)
        {
            int? blobId = blobTokenParser.ParseSystemBlobToken(token);

            if (blobId == null)
            {
                return new ContentResult()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain;charset=UTF-8",
                    Content = "Линкът за сваляне е изтекъл или невалиден. Моля, презаредете страницата и опитайте отново.",
                };
            }

            return new ProfileBlobStreamResult(BlobConstants.SystemProfileId, blobId.Value);
        }

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

        [Authorize(Policy = Policies.WriteProfileBlob)]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Route("profile/{profileId:int}")]
        public async Task<ActionResult<BlobDO>> PostProfileBlobAsync(
            [FromRoute] int profileId,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobReader blobReader,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
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

            int loginId = httpContextAccessor.HttpContext!.GetLoginId()!.Value;

            return await this.PostProfileBlobInternalAsync(
                profileId,
                loginId,
                BlobWriter.ProfileBlobAccessKeyType.Storage,
                true,
                loggerFactory,
                blobWriter,
                httpContextAccessor,
                ct);
        }

        [Authorize(Policy = Policies.WriteProfileBlob)]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Route("profile/{profileId:int}/temporary")]
        public async Task<ActionResult<BlobDO>> PostProfileBlobTemporaryAsync(
            [FromRoute] int profileId,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            int loginId = httpContextAccessor.HttpContext!.GetLoginId()!.Value;

            return await this.PostProfileBlobInternalAsync(
                profileId,
                loginId,
                BlobWriter.ProfileBlobAccessKeyType.Temporary,
                true,
                loggerFactory,
                blobWriter,
                httpContextAccessor,
                ct);
        }

        [Authorize(Policy = Policies.WriteSystemRegistrationBlob)]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Route("system/registration")]
        public async Task<ActionResult<BlobDO>> PostRegistrationBlobAsync(
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            return await this.PostProfileBlobInternalAsync(
                BlobConstants.SystemProfileId,
                httpContextAccessor.HttpContext!.GetLoginId()!.Value,
                BlobWriter.ProfileBlobAccessKeyType.Registration,
                true,
                loggerFactory,
                blobWriter,
                httpContextAccessor,
                ct);
        }

        [Authorize(Policy = Policies.WriteSystemTemplateBlob)]
        [HttpPost]
        [DisableFormValueModelBinding]
        [Route("system/template")]
        public async Task<ActionResult<BlobDO>> PostTemplateBlobAsync(
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            return await this.PostProfileBlobInternalAsync(
                BlobConstants.SystemProfileId,
                httpContextAccessor.HttpContext!.GetLoginId()!.Value,
                BlobWriter.ProfileBlobAccessKeyType.Template,
                false,
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
