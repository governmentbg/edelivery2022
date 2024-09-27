using System;
using System.Xml;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesRedirect;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOS.MessagesReceive
{
    public class ReceiveSeosMessage
    {
        public string Submit(string request, ILog logger)
        {
            logger.Info("Received message");
            MessageCreationSettings settings = null;

            try
            {
                var xsdValidate = new XsdValidationHelper(request, logger);
                xsdValidate.Validate();

                XmlDocument xmlRequest = ValidateXml(request, logger);

                Message message = DeserializeMessage(request, logger);
                settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(message);

                LogMessagesHelper.LogCommunication(message.Header.MessageGUID,
                    message.Header.MessageType, request, true, true, logger);

                //Redirect to AS4 node
                if (DatabaseQueries.IsAS4Entity(message.Header.Recipient.Identifier))
                {
                    var redirect = new RedirectFromSeosToAs4();
                    return redirect.Redirect(message, request, logger);
                }

                CheckIsThroughEDelivery(message, logger);

                var sender = CheckIsSenderActive(message, logger);

                var msgCertificateSN = ValidateXmlSignature(message, xmlRequest, request, sender.Id, logger);

                CompareCertificateSN(message, sender.CertificateSN, msgCertificateSN, logger);

                CheckMessageExists(message, logger);

                var response = ProcessRequestMessage(message, logger);
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    response.Type, response.Result, false, false, logger);
                return response.Result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing received message GUID {settings?.MessageGuid}", ex);
                var response = MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    ErrorsHelper.Describe(ex),
                    logger);

                SaveInRawFormatHelper.SaveRequest(
                    request, 
                    String.Empty, 
                    settings != null ? settings.MessageGuid : String.Empty, 
                    logger);

                return response;
            }
        }

        protected XmlDocument ValidateXml(string request, ILog logger)
        {
            XmlDocument xmlRequest;
            if (!request.LoadIfValidXmlDocument(out xmlRequest))
            {
                SaveInRawFormatHelper.SaveRequest(request, (int?)null,
                    nameof(Resources.Resource.InvalidXmlDocument), logger);
                throw new ApplicationException(Resources.Resource.InvalidXmlDocument);
            }
            return xmlRequest;
        }

        protected Message DeserializeMessage(string request, ILog logger)
        {
            Message message = request.DeserializeToMessage();
            if (message == null)
            {
                SaveInRawFormatHelper.SaveRequest(request, (int?)null,
                    nameof(Resources.Resource.InvalidXSDSchema), logger);
                throw new ApplicationException(Resources.Resource.InvalidXSDSchema);
            }
            return message;
        }

        protected bool CheckIsThroughEDelivery(Message message, ILog logger)
        {
            if (!DatabaseQueries.HasEDeliverySeos(message.Header.Recipient.Identifier))
            {
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID,
                    MessageType.MSG_Error, Resources.Resource.InvalidReceiver, false, false, logger);
                throw new ApplicationException(Resources.Resource.InvalidReceiver);
            }
            return true;
        }

        protected RegisteredEntity CheckIsSenderActive(Message message, ILog logger)
        {
            var sender = DatabaseQueries.GetRegisteredEntity(Guid.Parse(message.Header.Sender.GUID));
            if (sender == null || sender.Status != (int)SEOS.DBEntities.eEntityServiceStatus.Active)
            {
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    MessageType.MSG_Error, Resources.Resource.InvalidSender, false, false, logger);
                throw new ApplicationException(Resources.Resource.InvalidSender);
            }
            return sender;
        }

        protected string ValidateXmlSignature(Message message, XmlDocument xmlRequest, string request, int senderId, ILog logger)
        {
            string serialNumber;
            if (!SignedXmlHelper.ValidateXmlSignature(xmlRequest, out serialNumber))
            {
                SaveInRawFormatHelper.SaveRequest(request, senderId, 
                    nameof(Resources.Resource.InvalidMessageSignature), logger);
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    MessageType.MSG_Error, Resources.Resource.InvalidMessageSignature, false, false, logger);
                throw new ApplicationException(Resources.Resource.InvalidMessageSignature);
            }
            return serialNumber;
        }

        protected bool CompareCertificateSN(Message message, string entityCertificateSN, string msgCertificateSN, ILog logger)
        {
            if (entityCertificateSN != msgCertificateSN)
            {
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    MessageType.MSG_Error, Resources.Resource.InvalidMessageSignatureCertificate, false, false, logger);
                throw new ApplicationException(Resources.Resource.InvalidMessageSignatureCertificate);
            }
            return true;
        }

        protected bool CheckMessageExists(Message message, ILog logger)
        {
            var msgGuid = Guid.Parse(message.Header.MessageGUID);
            var seosMessage = DatabaseQueries.GetMessage(msgGuid, true, false);
            if (seosMessage != null)
            {
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    MessageType.MSG_Error, Resources.Resource.DuplicateMessageGuid, false, false, logger);
                throw new ApplicationException(Resources.Resource.DuplicateMessageGuid);
            }
            return false;
        }

        public MessageCreationResult ProcessRequestMessage(Message requestMessage, ILog logger)
        {
            logger.InfoFormat("ProcessResponse for message GUID {0}", requestMessage.Header.MessageGUID);
            var messageHandler = MessageFactory.CreateInstance(requestMessage.Header.MessageType);
            if (messageHandler == null)
                throw new ApplicationException($"Unknown message type GUID {requestMessage.Header.MessageGUID}");

            var message = messageHandler.Receive(requestMessage, logger);
            logger.Info("Response is: " + message.Result);
            return message;
        }
    }
}
