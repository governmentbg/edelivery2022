using EDelivery.SEOS.Models;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Utils;
using System;
using log4net;

namespace EDelivery.SEOS.MessagesRedirect
{
    public class RedirectFromAs4ToSeos : SubmitSeosMessage, IRedirectMessage
    {
        public RedirectFromAs4ToSeos()
            : base(false)
        {
        }

        public RedirectStatusRequestResult Redirect(string receiverUic, MsgData message, ILog logger)
        {
            var properties = new SendMessageProperties();
            var messageType = MessageType.MSG_DocumentRegistrationRequest;

            try
            {
                logger.Info($"Redirect As4 message GUID = {message.l1MessageId} " +
                    $"to Old SEOS with UIC = {receiverUic}");

                var entity = DatabaseQueries.GetRegisteredEntity(receiverUic);
                if (entity == null)
                {
                    throw new ApplicationException("There is no recipient with the specified UIC!");
                }

                Message innerMessage = message.payload.DeserializeToMessage();
                if (innerMessage == null)
                {
                    throw new ApplicationException(Resources.Resource.InvalidXSDSchema);
                }

                As4Helper.CheckSenderCoincide(message.l1From, innerMessage.Header.Sender.Identifier);

                properties = MapperHelper.Mapping.Map<Message, SendMessageProperties>(innerMessage);
                properties.ReceiverId = entity.Id;
                properties.ReceiverServiceUrl = entity.ServiceUrl;
                properties.MessageXml = SignedXmlHelper.SignMessageWithEDeliveryCertificate(innerMessage);
                messageType = innerMessage.Header.MessageType;

                var result = base.Submit(properties, messageType, logger);

                DatabaseQueries.SaveAs4TransferLog(innerMessage, message.l1MessageId, TransferTypeEnum.FromAS4toSEOS);
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
                logger.Error($"Redirect As4 message GUID = {message.l1MessageId}, " +
                    $"MessageGUID = {properties.MessageGuid}, " +
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
                var entity = DatabaseQueries.GetEDeliveryEntity();
                settings.Receiver = MapperHelper.Mapping
                    .Map<RegisteredEntity, EntityNodeType>(entity);

                return MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    statusResult.Error,
                    logger);
            }

            if (!String.IsNullOrEmpty(statusResult.StatusResponseMessage))
            {
                return statusResult.StatusResponseMessage;
            }

            return String.Empty;
        }
    }
}
