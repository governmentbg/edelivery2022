using System;
using System.Xml;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOS.MessagesReceive
{
    public class ReceiveAs4Message : ReceiveSeosMessage
    {
        public string Receive(MsgData seosMessage, ILog logger)
        {
            logger.Info("Redirect AS4 message");
            MessageCreationSettings settings = null;

            try
            {
                string request = seosMessage.payload;
                XmlDocument xmlRequest = ValidateXml(request, logger);

                Message message = DeserializeMessage(request, logger);
                settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(message);

                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    message.Header.MessageType, request, true, true, logger);

                CheckIsThroughEDelivery(message, logger);

                var sender = CheckIsSenderActive(message, logger);

                var msgCertificateSN = ValidateXmlSignature(message, xmlRequest, request, sender.Id, logger);

                //Проверката трябва да е по истинския сертификат, записан в Регистъра на AS4 участниците
                //CompareCertificateSN(message, sender.CertificateSN, msgCertificateSN, logger);

                CheckMessageExists(message, logger);

                var response = ProcessRequestMessage(message, logger);
                LogMessagesHelper.LogCommunication(message.Header.MessageGUID, 
                    response.Type, response.Result, false, false, logger);
                return response.Result;
            }
            catch (Exception ex)
            {
                logger.Error($"Error processing redirect message AS4GUID = {seosMessage.l1MessageId}, " +
                    $"sender {seosMessage.originalSender}, receiver {seosMessage.finalRecepient} ", ex);
                var response = MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    ex.Message,
                    logger);

                return response;
            }
        }
    }
}
