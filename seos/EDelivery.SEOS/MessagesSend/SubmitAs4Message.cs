using System;
using System.Web.Configuration;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOS.MessagesSend
{
    public class SubmitAs4Message : ISubmitMessage
    {
        private string ReceiverAs4 { get; set; }

        private string OriginalSender { get; set; }

        private string EDeliveryAs4 { get; set; }

        public SubmitAs4Message(string receiverAS4)
        {
            this.EDeliveryAs4 = WebConfigurationManager.AppSettings["EDeliveryAS4Name"];
            this.ReceiverAs4 = receiverAS4;
            this.OriginalSender = this.EDeliveryAs4;
        }

        public SubmitAs4Message(string receiverAS4, string originalSender)
            : this(receiverAS4)
        {
            this.OriginalSender = originalSender;
        }

        public SubmitStatusRequestResult Submit(SendMessageProperties properties, MessageType type, ILog logger)
        {
            try
            {
                var serviceResult = As4Helper.SendMessage(
                    properties.MessageXml,
                    this.EDeliveryAs4, 
                    this.ReceiverAs4, 
                    this.OriginalSender);
                var messageId = serviceResult.Request;

                if (!string.IsNullOrEmpty(serviceResult.ErrorMessage))
                {
                    var errorMsg = String.Format(
                        Resources.Resource.SubmitAs4MessageError,
                        messageId,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type, 
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName,
                        serviceResult.ErrorMessage);
                    logger.Error(errorMsg);

                    SaveInRawFormatHelper.SaveFaildRequest(properties.MessageXml, 
                        properties.Receiver?.AdministrativeBodyName, "Exception-SENT_FAILED", logger);
                    LogMessagesHelper.LogCommunication(properties.MessageGuid, 
                        MessageType.MSG_Error, serviceResult.ErrorMessage, null, false, true, logger);
                    return new SubmitStatusRequestResult
                    {
                        Error = serviceResult.ErrorMessage,
                        Status = DocumentStatusType.DS_SENT_FAILED,
                        StatusResponse = null
                    };
                }

                if (String.IsNullOrEmpty(messageId))
                {
                    var errorMsg = String.Format(
                        Resources.Resource.SubmitAs4MessageError,
                        messageId,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type,
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName,
                        "Empty AS4 message id");
                    logger.Error(errorMsg);

                    LogMessagesHelper.LogCommunication(properties.MessageGuid,
                        MessageType.MSG_Error, "AS4-" + messageId, null, false, true, logger);
                    return new SubmitStatusRequestResult
                    {
                        Error = "Empty AS4 message id",
                        Status = DocumentStatusType.DS_SENT_FAILED,
                        StatusResponse = null
                    };
                }

                var infoMsg = String.Format(
                        Resources.Resource.SubmitAs4MessageSuccess,
                        messageId,
                        properties.MessageGuid,
                        type);
                logger.Info(infoMsg);

                LogMessagesHelper.LogCommunication(properties.MessageGuid, 
                    MessageType.MSG_DocumentStatusResponse, "AS4-" + messageId, null, false, true, logger);
                LogTransfer(properties, messageId);

                return new SubmitStatusRequestResult
                {
                    Error = String.Empty,
                    Status = DocumentStatusType.DS_WAIT_REGISTRATION,
                    StatusResponse = null
                };
            }
            catch (Exception ex)
            {
                var errorMsg = String.Format(
                        Resources.Resource.SubmitAs4MessageError,
                        String.Empty,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type,
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName,
                        ex.Message);
                logger.Error(errorMsg);

                SaveInRawFormatHelper.SaveFaildRequest(properties.MessageXml, 
                    properties.Receiver?.AdministrativeBodyName, "Exception-SENT_FAILED", logger);
                LogMessagesHelper.LogCommunication(properties.MessageGuid, 
                    null, ex.ToString(), null, false, true, logger);
                return new SubmitStatusRequestResult
                {
                    Error = ex.Message,
                    Status = DocumentStatusType.DS_SENT_FAILED,
                    StatusResponse = null
                };
            }
        }

        public SubmitStatusRequestResult Submit(SEOSMessage message, MessageType type, ILog logger)
        {
            var seosMessage = MsgDocumentRegistrationRequest.Create(message, logger);
            LogMessagesHelper.LogCommunication(message.MessageGuid, MessageType.MSG_DocumentRegistrationRequest, seosMessage, message.Receiver.ServiceUrl, true, false, logger);

            var properties = MapperHelper.Mapping.Map<SEOSMessage, SendMessageProperties>(message);
            properties.MessageXml = seosMessage;

            return Submit(properties, type, logger);
        }

        private void LogTransfer(SendMessageProperties properties, string messageId)
        {
            var innerMessage = properties.MessageXml.DeserializeToMessage();
            if (innerMessage == null)
                return;

            if (properties.Sender == null)
                return;

            DatabaseQueries.SaveAs4SentMessage(innerMessage, messageId);

            if (RegisteredEntitiesHelper.IsThroughEDelivery(properties.Sender.Identifier) ||
                DatabaseAccess.DatabaseQueries.IsAS4Entity(properties.Sender.Identifier))
                return;

            DatabaseQueries.SaveAs4TransferLog(innerMessage, messageId, TransferTypeEnum.FromSEOStoAS4);
        }
    }
}
