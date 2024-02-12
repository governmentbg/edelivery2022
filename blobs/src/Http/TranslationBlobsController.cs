using System;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static ED.Blobs.BlobWriter;

namespace ED.Blobs
{
    [ApiController]
    public class TranslationBlobsController
    {
        [AllowAnonymous]
        [HttpPost("translations/callback")]
        public async Task<ActionResult<BlobDO>> PostDocumentCallback(
            [FromQuery(Name = "t")] string token,
            [FromQuery(Name = "request-id")] string? requestIdStr,
            [FromQuery(Name = "target-language")] string? targetLanguage,
            [FromServices] BlobTokenParser blobTokenParser,
            [FromServices] ILoggerFactory loggerFactory,
            [FromServices] BlobReader blobReader,
            [FromServices] BlobWriter blobWriter,
            [FromServices] IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            BlobTokenParser.ParsedTranslationBlobToken? parsedToken =
                blobTokenParser.ParseTranslationBlobToken(token);

            if (parsedToken == null)
            {
                return new ContentResult()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain;charset=UTF-8",
                    Content = "Линкът за сваляне е изтекъл или невалиден.",
                };
            }

            if (!long.TryParse(requestIdStr, out long requestId)
                || string.IsNullOrEmpty(targetLanguage))
            {
                throw new Exception("Missing translation parameters.");
            }

            BlobReader.MessageTranslationRequestInfoVO? messageTranslationRequestInfo =
                await blobReader.MessageTranslationRequestInfoAsync(
                    parsedToken.MessageTranslationId,
                    requestId,
                    targetLanguage,
                    ct) ?? throw new Exception("Missing message translation request.");

            string blobFileName = string.IsNullOrEmpty(messageTranslationRequestInfo.FileName)
                ? $"{targetLanguage}_Message_Content.txt"
                : $"{targetLanguage}_{messageTranslationRequestInfo.FileName}";

            BlobDO blob = await this.PostProfileBlobInternalAsync(
                messageTranslationRequestInfo.ProfileId,
                BlobConstants.AnonymousLoginId,
                blobFileName,
                BlobWriter.ProfileBlobAccessKeyType.Translation,
                loggerFactory,
                blobWriter,
                httpContextAccessor,
                ct);

            if (blob.BlobId == null)
            {
                await blobWriter.UpdateMessageTranslationRequestAsync(
                    parsedToken.MessageTranslationId,
                    requestId,
                    targetLanguage,
                    null,
                    4,
                    "Unsuccessful translation",
                    ct);
            }
            else
            {
                await blobWriter.UpdateMessageTranslationRequestAsync(
                    parsedToken.MessageTranslationId,
                    requestId,
                    targetLanguage,
                    blob.BlobId,
                    3,
                    null,
                    ct);
            }

            return blob;
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

        private async Task<BlobDO> PostProfileBlobInternalAsync(
            int profileId,
            int loginId,
            string fileName,
            BlobWriter.ProfileBlobAccessKeyType type,
            ILoggerFactory loggerFactory,
            BlobWriter blobWriter,
            IHttpContextAccessor httpContextAccessor,
            CancellationToken ct)
        {
            try
            {
                HttpContext httpContext = httpContextAccessor.HttpContext!;
                HttpRequest request = httpContext.Request;
                using FromBase64Transform transform = new();

                using CryptoStream decryptionStream =
                   new(request.Body, transform, CryptoStreamMode.Read);

                BlobInfo? blobInfo = null;

                blobInfo = await blobWriter.UploadProfileBlobAsync(
                    decryptionStream,
                    fileName,
                    long.MaxValue,
                    false,
                    profileId,
                    loginId,
                    type,
                    null,
                    ct);

                if (blobInfo == null || fileName == null)
                {
                    throw new Exception("Upload failed");
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
                loggerFactory.CreateLogger<BlobsController>().LogDebug("Translation upload cancelled");
                throw;
            }
        }
    }
}
