using System;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOS.MessagesSend
{
    public class SubmitSeosMessage : ISubmitMessage
    {
        private bool IsRetry { get; set; }

        public SubmitSeosMessage(bool isRetry)
        {
            this.IsRetry = isRetry;
        }

        public SubmitStatusRequestResult Submit(SendMessageProperties properties, MessageType type, ILog logger)
        {
            try
            {
                var serviceResult = EndPointHelper.SubmitEndpointMessage(properties.ReceiverServiceUrl, properties.MessageXml, logger);

                if (!string.IsNullOrEmpty(serviceResult.ErrorMessage))
                {
                    var errorMsg = String.Format(
                        Resources.Resource.SubmitMessageError,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type,
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName,
                        serviceResult.ErrorMessage);
                    logger.Error(errorMsg);

                    if (this.IsRetry && serviceResult.Status == DocumentStatusType.DS_SENT_FAILED)
                        SaveInRawFormatHelper.SaveFaildRequest(properties.MessageXml, properties.ReceiverId, "Exception-SENT_FAILED", logger);

                    if (this.IsRetry && serviceResult.Status == DocumentStatusType.DS_TRY_SEND)
                        RetryQueueHelper.AddMessage(properties.Id, properties.ReceiverId, properties.MessageXml, logger);

                    LogMessagesHelper.LogCommunication(properties.MessageGuid,
                        MessageType.MSG_Error, serviceResult.ErrorMessage, null, false, true, logger);
                    return new SubmitStatusRequestResult
                    {
                        Error = serviceResult.ErrorMessage,
                        Status = (DocumentStatusType)serviceResult.Status,
                        StatusResponse = null
                    };
                }

                if (string.IsNullOrEmpty(serviceResult.Request))
                {
                    LogMessagesHelper.LogCommunication(properties.MessageGuid,
                        MessageType.MSG_DocumentStatusResponse, serviceResult.Request, null, false, true, logger);
                    return new SubmitStatusRequestResult
                    {
                        Error = String.Empty,
                        Status = DocumentStatusType.DS_WAIT_REGISTRATION,
                        StatusResponse = null
                    };
                }

                bool isValidRespnse = EndPointHelper.ValidateEndpointResponse(serviceResult.Request, logger);
                if (!isValidRespnse)
                {
                    var errorMsg = String.Format(
                        Resources.Resource.SubmitMessageValidateRespnseError,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type,
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName);
                    logger.Error(errorMsg);

                    LogMessagesHelper.LogCommunication(
                        properties.MessageGuid,
                        null, 
                        "Result signature can not be validated! - " + serviceResult.Request, 
                        null, 
                        false, 
                        true, 
                        logger);
                }

                var resultMessage = serviceResult.Request.DeserializeToMessage();
                if (resultMessage.Header.MessageType == MessageType.MSG_Error)
                {
                    var errorMsg = String.Format(
                        Resources.Resource.SubmitMessageError,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type,
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName,
                        (resultMessage.Body.Item as ErrorMessageType).ErrorDescription);
                    logger.Error(errorMsg);

                    LogMessagesHelper.LogCommunication(properties.MessageGuid,
                        MessageType.MSG_Error, serviceResult.Request, null, false, true, logger);
                    return new SubmitStatusRequestResult
                    {
                        Error = (resultMessage.Body.Item as ErrorMessageType).ErrorDescription,
                        Status = DocumentStatusType.DS_SENT_FAILED,
                        StatusResponse = null
                    };
                }

                if (resultMessage.Header.MessageType == MessageType.MSG_DocumentStatusResponse)
                {
                    LogMessagesHelper.LogCommunication(properties.MessageGuid,
                        MessageType.MSG_DocumentStatusResponse, serviceResult.Request, null, false, true, logger);
                    var resBody = (resultMessage.Body.Item as DocumentStatusResponseType);
                    return new SubmitStatusRequestResult
                    {
                        Error = String.Empty,
                        Status = resBody.DocRegStatus,
                        StatusResponse = resBody,
                        StatusResponseMessage = serviceResult.Request
                    };
                }
                return new SubmitStatusRequestResult();
            }
            catch (Exception ex)
            {
                var errorMsg = String.Format(
                        Resources.Resource.SubmitMessageError,
                        properties.MessageGuid,
                        properties.DocIdentity?.DocumentGUID,
                        type,
                        properties.Sender?.AdministrativeBodyName,
                        properties.Receiver?.AdministrativeBodyName,
                        ex.Message);
                logger.Error(errorMsg);

                SaveInRawFormatHelper.SaveFaildRequest(properties.MessageXml, properties.ReceiverId, "Exception-SENT_FAILED", logger);
                LogMessagesHelper.LogCommunication(properties.MessageGuid,
                    MessageType.MSG_Error, ex.ToString(), null, false, true, logger);
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
    }
}
