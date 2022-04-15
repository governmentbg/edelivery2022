using EDelivery.SEOS.Models;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Utils;
using System;
using log4net;

namespace EDelivery.SEOS.MessagesRedirect
{
    public class RedirectFromAs4ToAs4: SubmitAs4Message, IRedirectMessage
    {
        public RedirectFromAs4ToAs4(string receiverAS4, string originalSender)
            :base(receiverAS4, originalSender)
        {
        }

        public RedirectStatusRequestResult Redirect(string receiverUic, MsgData message, ILog logger)
        {
            var properties = new SendMessageProperties();
            var messageType = MessageType.MSG_DocumentRegistrationRequest;

            try
            {
                logger.Info($"Redirect of As4 message GUID = {message.l1MessageId} " +
                    $"to As4 node {message.finalRecepient} with UIC = {receiverUic}");

                Message innerMessage = message.payload.DeserializeToMessage();
                if (innerMessage != null)
                {
                    properties = MapperHelper.Mapping.Map<Message, SendMessageProperties>(innerMessage);
                    messageType = innerMessage.Header.MessageType;

                    As4Helper.CheckSenderCoincide(message.l1From, 
                        innerMessage.Header.Sender.Identifier);
                    As4Helper.CheckReceiverCoincide(message.finalRecepient, 
                        innerMessage.Header.Recipient.Identifier);
                }
                properties.MessageXml = message.payload;
                

                var result = base.Submit(properties, messageType, logger);

                DatabaseQueries.SaveAs4TransferLog(innerMessage, message.l1MessageId, TransferTypeEnum.FromAS4toAS4);
                return new RedirectStatusRequestResult
                {
                    Error = result.Error,
                    Status = result.Status,
                    StatusResponse = result.StatusResponse,
                    StatusResponseMessage = result.StatusResponseMessage,
                    MsgType = messageType,
                    MessageGuid = properties.MessageGuid,  
                    DocIdentity = properties.DocIdentity,
                    Sender = properties.Sender,
                    Receiver = properties.Receiver
                };
            }
            catch (Exception ex)
            {
                logger.Error($"Redirect As4 message GUID = {message.l1MessageId}," +
                    $" MessageGUID = {properties.MessageGuid}, " +
                    $"docGuid {properties.DocIdentity?.DocumentGUID}, " +
                    $"sender {properties.Sender?.AdministrativeBodyName}, " +
                    $"receiver {properties.Receiver?.AdministrativeBodyName} failed with Exception. No retries", ex);
                return new RedirectStatusRequestResult
                {
                    Error = ex.Message,
                    Status = DocumentStatusType.DS_SENT_FAILED,
                    StatusResponse = null,
                    MsgType = messageType,
                    MessageGuid = properties.MessageGuid,
                    DocIdentity = properties.DocIdentity,
                    Sender = properties.Sender,
                    Receiver = properties.Receiver
                };
            }
        }

        public string CreateRedirectStatusResponse(
            RedirectStatusRequestResult statusResult,
            ILog logger)
        {
            if (statusResult.Sender == null || statusResult.Receiver == null)
                return String.Empty;

            var settings = new MessageCreationSettings
            {
                MessageGuid = statusResult.MessageGuid.ToString("B"),
                DocIdentity = statusResult.DocIdentity,
                Sender = statusResult.Sender,
                Receiver = statusResult.Receiver,
                RejectionReason = String.Empty,
                DocExpectCloseDateSpecified = false,
                DocExpectCloseDate = DateTime.MinValue
            };

            if (!String.IsNullOrEmpty(statusResult.Error))
            {
                return MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    statusResult.Error,
                    logger);
            }

            if (statusResult.MsgType == MessageType.MSG_DocumentRegistrationRequest ||
                statusResult.MsgType == MessageType.MSG_DocumentStatusRequest)
            {
                //Fake response
                return MsgDocumentStatusResponse.Create(
                    settings,
                    DocumentStatusType.DS_WAIT_REGISTRATION);
            }

            return String.Empty;
        }
    }
}
