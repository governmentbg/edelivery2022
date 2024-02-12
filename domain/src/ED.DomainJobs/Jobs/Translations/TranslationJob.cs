using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ED.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using MediatR;
using System.Collections.Generic;
using System.Text;

namespace ED.DomainJobs
{
    public class TranslationJob : QueueJob<TranslationQueueMessage, DisposableTuple<ETranslationClient, IServiceScope>>
    {
        private int MaxFileSize = 20 * 1024 * 1024;

        private readonly string[] SupportedFiles = new string[]
        {
            "odt",
            "ods",
            "odp",
            "odg",
            "ott",
            "ots",
            "otp",
            "otg",
            "rtf",
            "doc",
            "docx",
            "xls",
            "xlsx",
            "ppt",
            "ppts",
            "pdf",
            "txt",
            "htm",
            "html",
            "xhtml",
            "xml",
            "xlf",
            "xliff",
            "sdlxliff",
            "tmx",
            "rdf",
        };

        private readonly Dictionary<long, string> TranslationErrors = new()
        {
            { -2, "Unexpected response" },
            { -20000, "Source language not specified" },
            { -20001,  "Invalid source language" },
            { -20002,  "Target language(s) not specified" },
            { -20003,  "Invalid target language(s)" },
            { -20004,  "DEPRECATED" },
            { -20005,  "Caller information not specified" },
            { -20006,  "Missing application name" },
            { -20007,  "Application not authorized to access the service" },
            { -20008,  "Bad format for ftp address" },
            { -20009,  "Bad format for sftp address" },
            { -20010,  "Bad format for http address" },
            { -20011,  "Bad format for email address" },
            { -20012,  "Translation request must be text type, document path type or document base64 type and not several at a time" },
            { -20013,  "Language pair not supported by the domain" },
            { -20014,  "Username parameter not specified" },
            { -20015,  "Extension invalid compared to the MIME type" },
            { -20016,  "DEPRECATED" },
            { -20017,  "Username parameter too long" },
            { -20018,  "Invalid output format" },
            { -20019,  "Institution parameter too long" },
            { -20020,  "Department number too long" },
            { -20021,  "Text to translate too long" },
            { -20022,  "Too many FTP destinations" },
            { -20023,  "Too many SFTP destinations" },
            { -20024,  "Too many HTTP destinations" },
            { -20025,  "Missing destination" },
            { -20026,  "Bad requester callback protocol" },
            { -20027,  "Bad error callback protocol" },
            { -20028,  "Concurrency quota exceeded" },
            { -20029,  "Document format not supported" },
            { -20030,  "Text to translate is empty" },
            { -20031,  "Missing text or document to translate" },
            { -20032,  "Email address too long" },
            { -20033,  "Cannot read stream" },
            { -20034,  "Output format not supported" },
            { -20035,  "Email destination tag is missing or empty" },
            { -20036,  "HTTP destination tag is missing or empty" },
            { -20037,  "FTP destination tag is missing or empty" },
            { -20038,  "SFTP destination tag is missing or empty" },
            { -20039,  "Document to translate tag is missing or empty" },
            { -20040,  "Format tag is missing or empty" },
            { -20041,  "The content is missing or empty" },
            { -20042,  "Source language defined in TMX file differs from request" },
            { -20043,  "Source language defined in XLIFF file differs from request" },
            { -20044,  "Output format is not available when quality estimate is requested. It should be blank or 'xslx'" },
            { -20045,  "Quality estimate is not available for text snippet" },
            { -20046,  "Document too big (>20Mb)" },
            { -20047,  "Quality estimation not available" },
            { -40010,  "Too many segments to translate" },
            { -80004,  "Cannot store notification file at specified FTP address" },
            { -80005,  "Cannot store notification file at specified SFTP address" },
            { -80006,  "Cannot store translated file at specified FTP address" },
            { -80007,  "Cannot store translated file at specified SFTP address" },
            { -90000,  "Cannot connect to FTP" },
            { -90001,  "Cannot retrieve file at specified FTP address" },
            { -90002,  "File not found at specified address on FTP" },
            { -90007,  "Malformed FTP address" },
            { -90012,  "Cannot retrieve file content on SFTP" },
            { -90013,  "Cannot connect to SFTP" },
            { -90014,  "Cannot store file at specified FTP address" },
            { -90015,  "Cannot retrieve file content on SFTP" },
            { -90016,  "Cannot retrieve file at specified SFTP address" },
        };

        private IServiceScopeFactory scopeFactory;

        public TranslationJob(
           IServiceScopeFactory scopeFactory,
           ILogger<TranslationJob> logger,
           IOptions<DomainJobsOptions> optionsAccessor)
           : base(QueueMessageType.Translation, scopeFactory, logger, optionsAccessor.Value.TranslationJob)
        {
            this.scopeFactory = scopeFactory;
        }

        protected override Task<DisposableTuple<ETranslationClient, IServiceScope>> CreateThreadContextAsync(CancellationToken ct)
        {
            var scope = this.scopeFactory.CreateScope();
            var client = scope.ServiceProvider.GetRequiredService<ETranslationClient>();
            return Task.FromResult(DisposableTuple.Create(client, scope));
        }

        protected override async Task<(QueueJobProcessingResult result, string? error)>
            HandleMessageAsync(
                DisposableTuple<ETranslationClient, IServiceScope> context,
                TranslationQueueMessage payload,
                bool isLastAttempt,
                CancellationToken ct)
        {
            var client = context.Item1;

            try
            {
                // TODO add transaction
                string authorizationHeader =
                    await client.SubmitAuthorizationRequestAsync(ct);

                using IServiceScope scope = this.scopeFactory.CreateScope();

                IMediator mediator = scope.ServiceProvider.GetService<IMediator>()!;

                BlobsServiceClient blobsServiceClient =
                    scope.ServiceProvider.GetRequiredService<BlobsServiceClient>();

                BlobUrlCreator blobUrlCreator =
                    scope.ServiceProvider.GetRequiredService<BlobUrlCreator>();

                string blobAuthenticationToken =
                    blobUrlCreator.CreateBlobToken(
                        payload.MessageTranslationId);

                IJobsMessagesOpenHORepository jobsMessagesOpenHORepository =
                    scope.ServiceProvider.GetRequiredService<IJobsMessagesOpenHORepository>();

                IJobsTranslationsListQueryRepository jobsTranslationsListQueryRepository =
                    scope.ServiceProvider.GetRequiredService<IJobsTranslationsListQueryRepository>();

                IJobsTranslationsListQueryRepository.GetPendingTranslationRequestsVO[] requests =
                    await jobsTranslationsListQueryRepository.GetPendingTranslationRequestsAsync(
                        payload.MessageTranslationId,
                        ct);

                foreach (IJobsTranslationsListQueryRepository.GetPendingTranslationRequestsVO request in requests)
                {
                    if (!string.IsNullOrEmpty(request.FileName))
                    {
                        string blobFilenameExtension = Path.GetExtension(request.FileName).Replace(".", string.Empty);

                        if (!this.SupportedFiles.Contains(blobFilenameExtension))
                        {
                            _ = await mediator.Send(
                            new JobsUpdateMessageTranslationRequestCommand(
                                payload.MessageTranslationId,
                                request.SourceBlobId,
                                -20029,
                                MessageTranslationRequestStatus.Errored,
                                this.TranslationErrors[-20029]),
                            ct);

                            continue;
                        }
                        else if (request.Size == null || request.Size > this.MaxFileSize)
                        {
                            _ = await mediator.Send(
                            new JobsUpdateMessageTranslationRequestCommand(
                                payload.MessageTranslationId,
                                request.SourceBlobId,
                                -20046,
                                MessageTranslationRequestStatus.Errored,
                                this.TranslationErrors[-20046]),
                            ct);

                            continue;
                        }
                    }

                    // TODO use recyclableMemoryStreamManager if possible
                    byte[] content = null!;
                    string extension = null!;
                    string filename = null!;

                    if (request.SourceBlobId == null)
                    {
                        string messageContent =
                            await jobsMessagesOpenHORepository.GetAsRecipientAsync(
                                request.MessageId,
                                request.ProfileId, ct);

                        content = Encoding.UTF8.GetBytes(messageContent);
                        extension = "txt";
                        filename = "Message_Content";
                    }
                    else
                    {
                        BlobsServiceClient.DownloadBlobToArrayVO blobToArray =
                            await blobsServiceClient.DownloadMessageBlobToArrayAsync(
                                request.ProfileId,
                                request.SourceBlobId.Value,
                                request.MessageId,
                                ct);

                        content = blobToArray.Content;
                        extension = Path.GetExtension(request.FileName).Replace(".", string.Empty);
                        filename = Path.GetFileNameWithoutExtension(request.FileName);
                    }

                    string requestIdStr =
                        await client.SubmitDocumentAsync(
                            content,
                            extension,
                            filename,
                            request.SourceLanguage,
                            new string[] { request.TargetLanguage },
                            authorizationHeader,
                            blobAuthenticationToken,
                            ct);

                    if (long.TryParse(requestIdStr, out var requestId))
                    {
                        _ = await mediator.Send(
                            new JobsUpdateMessageTranslationRequestCommand(
                                payload.MessageTranslationId,
                                request.SourceBlobId,
                                requestId,
                                requestId < 0
                                    ? MessageTranslationRequestStatus.Errored
                                    : MessageTranslationRequestStatus.Processing,
                                requestId < 0
                                    ? this.TranslationErrors[requestId]
                                    : null),
                               ct);
                    }
                    else
                    {
                        _ = await mediator.Send(
                            new JobsUpdateMessageTranslationRequestCommand(
                                payload.MessageTranslationId,
                                request.SourceBlobId,
                                -2,
                                MessageTranslationRequestStatus.Errored,
                                this.TranslationErrors[-2]),
                            ct);
                    }
                }

                IQueueMessagesService queueMessagesService = scope.ServiceProvider
                    .GetRequiredService<IQueueMessagesService>();

                await queueMessagesService.TryPostMessageAndSaveAsync(
                    new TranslationClosureQueueMessage(
                        payload.MessageTranslationId),
                    DateTime.Now.AddHours(6),
                    null!,
                    ct);

                return (QueueJobProcessingResult.Success, null);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return (QueueJobProcessingResult.RetryError, ex.Message);
            }
        }
    }
}
