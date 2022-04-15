using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesAttachments;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.MessagesStatus;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOSPostService
{
    public class SEOSPostService : ISEOSPostService
    {
        protected static ILog logger = LogManager.GetLogger("SEOSPostLogger");

        public SEOSPostService()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public SendMessageResult SendSeos(string url, string message)
        {
            return EndPointHelper.SubmitEndpointMessage(url, message, logger);
        }

        public SendMessageResult SendAs4(string receiverAS4, string message)
        {
            var eDeliveryAS4Name = System.Web.Configuration.WebConfigurationManager.AppSettings["EDeliveryAS4Name"];
            return As4Helper.SendMessage(message, 
                eDeliveryAS4Name, receiverAS4, eDeliveryAS4Name);
        }

        public MessagesPageResponse GetReceivedDocuments(Guid eSubjectId, int page, int pageSize, SortColumnEnum sortColumn, SortOrderEnum sortOrder)
        {
            try
            {
                var result = DatabaseQueries.GetReceivedDocuments(eSubjectId,
                    page, pageSize, sortColumn, sortOrder == SortOrderEnum.Desc);
                return MapperHelper.Mapping
                    .Map<MessagesPage, MessagesPageResponse>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting sent messages", ex);
                throw;
            }
        }

        public MessagesPageResponse GetSentDocuments(Guid eSubjectId, int page, int pageSize, SortColumnEnum sortColumn, SortOrderEnum sortOrder)
        {
            try
            {
                var result = DatabaseQueries.GetSentDocuments(eSubjectId, 
                    page, pageSize, sortColumn, sortOrder == SortOrderEnum.Desc);
                return MapperHelper.Mapping
                    .Map<MessagesPage, MessagesPageResponse>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting received messages", ex);
                throw;
            }
        }

        public Dictionary<Guid, int> GetNewMessagesCount(List<Guid> profileGuidList)
        {
            try
            {
                return DatabaseQueries.GetNewMessagesCount(profileGuidList);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting new mwssages count", ex);
                throw;
            }
        }

        public OpenDocumentResponse OpenDocument(OpenDocumentRequest request)
        {
            try
            {
                var message = DatabaseQueries.GetMessage(request.MessageGuid, true, true);

                if (message == null)
                {
                    var exMessage = $"Message with ID {request.MessageGuid} not found";
                    return null;
                }

                if (request.IsReceived && message.ReceiverElectronicSubjectId != request.ESubjectId ||
                    !request.IsReceived && message.SenderElectronicSubjectId != request.ESubjectId)
                {
                    var exMessage = $"Unauthorized access exception. MessageId {request.MessageGuid}, " +
                        $"electronicSubjectId {request.ESubjectId}, isReceived {request.IsReceived}";
                    return null;
                }

                if (!request.IsReceived)
                    return MapperHelper.Mapping
                        .Map<SEOSMessage, OpenDocumentResponse>(message);

                //check if we open a message for the first time
                if (message.Status == (int)DocumentStatusType.DS_WAIT_REGISTRATION)
                {
                    message = DatabaseQueries.OpenReceivedDocumentAndRegister(message.Id,
                        request.ProfileName, request.ProfileGuid);

                    //if the sender is not from EDelivery
                    message = DatabaseQueries.GetMessage(message.MessageGuid, true, true);
                    Status.SubmitStatusUpdateNotification(message, logger);
                }

                var result = MapperHelper.Mapping
                    .Map<SEOSMessage, OpenDocumentResponse>(message);
                result.IsReceived = request.IsReceived;
                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error opening document", ex);
                throw;
            }
        }

        public List<RegisteredEntityResponse> GetRegisteredEntities()
        {
            try
            {
                var result = DatabaseQueries.GetRegisteredEntities();
                return MapperHelper.Mapping
                    .Map<List<RegisteredEntity>, List<RegisteredEntityResponse>>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting registered entities", ex);
                throw;
            }
        }

        public RegisteredEntityResponse GetEDeliveryEntity()
        {
            try
            {
                var result = DatabaseQueries.GetEDeliveryEntity();
                return MapperHelper.Mapping
                    .Map<RegisteredEntity, RegisteredEntityResponse>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting EDelivery Entity", ex);
                throw;
            }
        }

        public RegisteredEntityResponse GetRegisteredEntity(Guid entityId)
        {
            try
            {
                var result = DatabaseQueries.GetRegisteredEntity(entityId);
                return MapperHelper.Mapping
                    .Map<RegisteredEntity, RegisteredEntityResponse>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting particular registered entity", ex);
                throw;
            }
        }

        public Dictionary<string, bool> HasSeos(List<string> uicList)
        {
            try
            {
                return DatabaseQueries.HasEDeliverySeos(uicList);
            }
            catch (Exception ex)
            {
                logger.Error("Error getting seos permission", ex);
                throw;
            }
        }

        public DocumentStatusType? CheckDocumentStatus(Guid messageId, Guid eSubjectId)
        {
            try
            {
                var message = DatabaseQueries.GetMessageSender(messageId, eSubjectId);
                if (message == null)
                {
                    logger.Error($"CheckMessageStatus - Error getting message " +
                        $"with messageId {messageId} and receiverId {eSubjectId}");
                    return null;
                }

                //ask for status if the receiver is not through eDelivery
                if (!message.ReceiverElectronicSubjectId.HasValue)
                {
                    var statusResult = Status.SubmitStatusRequest(message, logger);
                    if (statusResult != null && string.IsNullOrEmpty(statusResult.Error))
                    {
                        logger.Info($"UpdateMessageStatus, messageId {message.Id}, " +
                            $"messageBody status {statusResult.StatusResponse.DocRegStatus}");
                        message = DatabaseQueries.ApplySentDocumentStatus(message.Id, statusResult);
                    }
                }

                return (DocumentStatusType)message.Status;
            }
            catch (Exception ex)
            {
                logger.Error("Error check message status", ex);
                throw;
            }
        }

        public ChangeDocumentStatusResponse GetDocumentStatus(Guid messageId, Guid eSubjectId)
        {
            try
            {
                var message = DatabaseQueries.GetMessageReceiver(messageId, eSubjectId);
                if (message == null)
                {
                    logger.Error($"GetDocumentStatus - Error getting message " +
                        $"with messageId {messageId} and receiverId {eSubjectId}");
                    return null;
                }

                return MapperHelper.Mapping
                    .Map<SEOSMessage, ChangeDocumentStatusResponse>(message);
            }
            catch (Exception ex)
            {
                logger.Error("Can not get message with messageGuid " + messageId.ToString(), ex);
                throw;
            }
        }

        public bool UpdateDocumentStatus(ChangeDocumentStatusRequest request)
        {
            try
            {
                var message = DatabaseQueries.GetMessageReceiver(request.MessageId, request.ProfileGuid);
                if (message == null)
                {
                    logger.Error($"UpdateMessageStatus - Error getting message with " +
                        $"messageId {request.MessageId} and receiverId {request.ProfileGuid}");
                    return false;
                }

                if (request.NewStatus == (DocumentStatusType)message.Status)
                    return true;

                message = DatabaseQueries.UpdateDocumentStatus(message.Id, 
                    request.NewStatus, request.ExpectedDateClose, request.RejectReason);
                if (message == null)
                    throw new ApplicationException("The status change message cannot be found");

                //if the sender is not from EDelivery
                Status.SubmitStatusUpdateNotification(message, logger);

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Error updating message status, messageId {0}, newStatus {1}", ex);
                throw;
            }
        }

        public AttachmentResponse GetDocumentAttachment(Guid? messageGuid, int attachmentId)
        {
            try
            {
                var result = DatabaseQueries.GetDocumentAttachment(messageGuid, attachmentId);
                return MapperHelper.Mapping
                    .Map<SEOSMessageAttachment, AttachmentResponse>(result);
            }
            catch (Exception ex)
            {
                logger.Error($"Error getting document attachment for messageGuid" +
                    $" {messageGuid}, attachmentId {attachmentId}", ex);
                throw;
            }
        }

        public MalwareScanResultResponse SaveAttachmentWithScan(AttachmentRequest doc)
        {
            var task = Task<MalwareScanResultResponse>
                .Run(async () => await AttachDocument.SaveTempDocumentWithScan(doc, logger));
            return task.Result;
        }

        public SubmitStatusRequestResult SendMessage(MessageRequest request, CorespondentRequest corespondent)
        {
            try
            {
                var attachments = AttachDocument.GetAttachments(request.DocumenAttachmentFirstContent,
                    request.DocumenAttachmentFirstComment, request.DocumenAttachments);

                var corespondents = new List<SEOSMessageCorespondent>()
                {
                    MapperHelper.Mapping
                    .Map<CorespondentRequest, SEOSMessageCorespondent>(corespondent)
                };

                var message = DatabaseQueries.CreateSendMessage(
                    request, corespondents, attachments);

                var submitHandler = SubmitMessageFactory.CreateInstance(
                    message.Receiver.EIK,
                    true,
                    message.ReceiverElectronicSubjectId.HasValue);

                var result = submitHandler.Submit(message,
                    MessageType.MSG_DocumentRegistrationRequest,
                    logger);

                DatabaseQueries.ApplySentDocumentStatus(message.Id, result);

                if (result.StatusResponse != null && result.StatusResponse.DocID != null)
                    result.StatusResponse.DocID.Item = null;

                return result;
            }
            catch (Exception ex)
            {
                logger.Error("Error sending message", ex);
                throw;
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
