using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using log4net;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MalwareService;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;

namespace EDelivery.SEOS.MessagesAttachments
{
    public class AttachDocument
    {
        public static async Task<MalwareScanResultResponse> SaveTempDocumentWithScan(AttachmentRequest document, ILog logger)
        {
            var malwareScanEnabled = bool.Parse(WebConfigurationManager.AppSettings["SEOS.MalwareScanEnabled"]);

            if (malwareScanEnabled == false)
            {
                logger.Warn("Malware scan is not configured or is not enabled");
                int tempFileId = await DatabaseQueries.CreateSEOSDocumentAttachmetn(document);
                return new MalwareScanResultResponse()
                {
                    DbItemId = tempFileId,
                    FileName = document.Name,
                    ErrorReason = MalwareScanErrorReasonEnum.MalwareScanDisabled
                };
            }

            var malwareScanResult = new MalwareScanResultResponse()
            {
                FileName = document.Name
            };

            try
            {
                //save the file in the database
                int tempFileId = await DatabaseQueries.CreateSEOSDocumentAttachmetn(document);
                document.Id = tempFileId;

                var scanResult = await ScanDoc(document);
                var settings = scanResult.Item2;

                //update the malware result in the database
                await DatabaseQueries.UpdateSEOSDocumentMalwareScanResult(tempFileId, settings);

                malwareScanResult.DbItemId = tempFileId;
                malwareScanResult.ErrorReason = settings.ErrorReason;
                malwareScanResult.IsSuccessfulScan = settings.IsSuccessfulScan;
                malwareScanResult.Message = settings.Message;
                malwareScanResult.Status = settings.Status;
                malwareScanResult.StatusDate = settings.StatusDate;
                malwareScanResult.ElapsedTimeSeconds = settings.ElapsedTimeSeconds;
                malwareScanResult.IsMalicious = settings.IsMalicious.HasValue
                    ? settings.IsMalicious.Value
                    : false;

                if (!String.IsNullOrEmpty(settings.InnerMessage) && !settings.IsSuccessfulScan)
                    logger.Error($"Error in SubmitDocumentsForMalwareScan " +
                        $"for {document.Name}: {settings.InnerMessage}");
            }
            catch (Exception ex)
            {
                logger.Error($"Error in SubmitDocumentsForMalwareScan for {document.Name}: ", ex);
                malwareScanResult.IsSuccessfulScan = false;
                malwareScanResult.Message = ex.Message;
                malwareScanResult.ErrorReason = MalwareScanErrorReasonEnum.EDeliveryError;
            }
            return malwareScanResult;
        }

        public static async Task<bool> SendDocumentsForMalwareScan(IEnumerable<SEOSMessageAttachment> documents, ILog logger)
        {
            var malwareScanEnabled = bool.Parse(WebConfigurationManager.AppSettings["SEOS.MalwareScanEnabled"]);
            var malwareApiUrl = WebConfigurationManager.AppSettings["SEOS.MalwareApiUrl"];

            if (malwareScanEnabled == false)
            {
                logger.Warn("MalwareScan is not configured or is diabled");
                return true;
            }

            if (documents == null || documents.Count() == 0)
            {
                return true;
            }

            try
            {
                var tasks = new List<Task<Tuple<int, MalwareScanSettings>>>();

                foreach (var document in documents)
                {
                    var request = MapperHelper.Mapping
                        .Map<SEOSMessageAttachment, AttachmentRequest>(document);
                    tasks.Add(ScanDoc(request));
                }

                foreach (var task in await Task.WhenAll(tasks))
                {
                    var dbResult = await DatabaseQueries.UpdateSEOSDocumentMalwareScanResult(task.Item1, task.Item2);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error in async method SendDocumentsForMalwareScan", ex);
                return false;
            }
        }

        public static List<SEOSMessageAttachment> GetAttachments(int firstContent, string firstComment, List<AttachmentRequest> attachments)
        {
            var result = new List<SEOSMessageAttachment>();

            var mainDoc = DatabaseQueries.GetDocumentAttachment(null, firstContent);
            mainDoc.Comment = firstComment;
            result.Add(mainDoc);

            foreach (var modelDoc in attachments)
            {
                if (!modelDoc.Id.HasValue)
                {
                    continue;
                }

                var doc = DatabaseQueries.GetDocumentAttachment(null, modelDoc.Id.Value);
                doc.Comment = modelDoc.Comment;
                result.Add(doc);
            }

            return result;
        }

        private static async Task<Tuple<int, MalwareScanSettings>> ScanDoc(AttachmentRequest document)
        {
            var serviceResult = new Evaluation();
            var id = document.Id.HasValue ? document.Id.Value : 0;
            try
            {
                using (var memoryStream = new MemoryStream(document.Content, 0, document.Content.Length))
                {
                    serviceResult = await ScanAsync(memoryStream, document.Name, default(CancellationToken));
                }
            }
            catch (Exception ex)
            {
                var errorReason = (ex is EvaluationServiceException)
                    ? MalwareScanErrorReasonEnum.ServiceReturnedError
                    : MalwareScanErrorReasonEnum.ApiNotAccessible;

                return (id, new MalwareScanSettings(errorReason, ex.Message)).ToTuple();
            }

            if (serviceResult.Status == EvaluationStatus.Error ||
                (serviceResult.Files?.Any() == true && serviceResult.Files[0].Status == EvaluationStatus.Error))
            {
                var errorReason = MalwareScanErrorReasonEnum.ServiceReturnedError;

                return (id, new MalwareScanSettings(serviceResult, errorReason)).ToTuple();
            }

            if (serviceResult.Status == EvaluationStatus.InProgress ||
                (serviceResult.Files?.Any() == true && serviceResult.Files[0].Status == EvaluationStatus.InProgress))
            {
                var errorReason = MalwareScanErrorReasonEnum.ServiceResultPending;

                return (id, new MalwareScanSettings(serviceResult, errorReason)).ToTuple();
            }

            if (serviceResult.Status == EvaluationStatus.Complete ||
                (serviceResult.Files?.Any() == true && serviceResult.Files[0].Status == EvaluationStatus.Complete))
            {
                var isMalicious = (serviceResult.Files == null
                    ? serviceResult.Malicious
                    : serviceResult.Files[0].Malicious)
                    ?? false;

                return (id, new MalwareScanSettings(serviceResult, true, isMalicious, serviceResult.Id)).ToTuple();
            }

            return (id, new MalwareScanSettings()).ToTuple();
        }

        private static async Task<Evaluation> ScanAsync(
            Stream file,
            string fileName,
            CancellationToken ct)
        {
            var malwareServiceClient = MalwareServiceClientFactory.CreateClient();

            var malwareScanId = await malwareServiceClient.SubmitAsync(
                file,
                fileName,
                null,
                ct);


            Evaluation result = null;
            var counter = 5;
            while (counter > 0)
            {
                result = await malwareServiceClient.GetAsync(malwareScanId, ct);

                if (result.Status != EvaluationStatus.InProgress ||
                    (result.Files != null && result.Files[0].Status != EvaluationStatus.InProgress))
                {
                    break;
                }

                counter--;
                if (counter > 0)
                {
                    await Task.Delay((5 - counter) * 1000, ct);
                }
            }
            return result;
        }
    }
}
