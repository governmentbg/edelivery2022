using System;
using log4net;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;

namespace EDelivery.SEOS.MessagesStatus
{
    public class Status
    {
        /// <summary>
        /// Makes a request for verification of the status of a document
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="logger">Logger</param>
        /// <returns></returns>
        public static SubmitStatusRequestResult SubmitStatusRequest(SEOSMessage message, ILog logger)
        {
            try
            {
                var settings = MapperHelper.Mapping
                    .Map<SEOSMessage, MessageCreationSettings>(message);
                var seosMessage = MsgDocumentStatusRequest.Create(
                    settings);

                LogMessagesHelper.LogCommunication(message.MessageGuid,
                    MessageType.MSG_DocumentStatusResponse, seosMessage, null, false, true, logger);

                logger.Info($"Check message status: messageGuid {message.MessageGuid} docGuid {message.DocGuid}");

                var properties = MapperHelper.Mapping
                    .Map<SEOSMessage, SendMessageProperties>(message);
                properties.MessageXml = seosMessage;

                var submitHandler = SubmitMessageFactory.CreateInstance(
                    properties.Receiver.Identifier,
                    false,
                    false);

                var result = submitHandler.Submit(properties,
                    MessageType.MSG_DocumentStatusRequest,
                    logger);

                return result;
            }
            catch (Exception ex)
            {
                LogMessagesHelper.LogCommunication(message.MessageGuid,
                    MessageType.MSG_DocumentStatusResponse, ex.ToString(), null, false, true, logger);
                logger.Error($"Error getting message status, docGuid {message.DocGuid}, reseiver {message.Sender.Name} ", ex);
                throw;
            }
        }

        /// <summary>
        /// Check message status
        /// </summary>
        /// <param name="message"></param>
        /// <param name="receiver"></param>
        /// <param name="rejectReason"></param>
        /// <returns></returns>
        public static void SubmitStatusUpdateNotification(SEOSMessage message, ILog logger)
        {
            //if the sender was  through edelvery, then submit the message
            if (message.SenderElectronicSubjectId.HasValue)
            {
                return;
            }

            try
            {
                logger.Info($"SubmitStatusUpdateNotification for docId {message.DocGuid}, receiver {message.Sender.Name}");

                var settings = MapperHelper.Mapping
                    .Map<SEOSMessage, MessageCreationSettings>(message);
                var seosMessage = MsgDocumentStatusResponse.Create(
                    settings,
                    (DocumentStatusType)message.Status);

                LogMessagesHelper.LogCommunication(message.MessageGuid,
                    MessageType.MSG_DocumentStatusResponse, seosMessage, message.Receiver.ServiceUrl, true, false, logger);
                logger.Info($"Update message status: messageGuid {message.MessageGuid} docGuid {message.DocGuid}");

                var properties = MapperHelper.Mapping
                    .Map<SEOSMessage, ReplyMessageProperties>(message);
                properties.MessageXml = seosMessage;

                var submitHandler = SubmitMessageFactory.CreateInstance(
                    properties.Receiver.Identifier,
                    false,
                    false);

                var result = submitHandler.Submit(properties,
                    MessageType.MSG_DocumentStatusResponse,
                    logger);

            }
            catch (Exception ex)
            {
                logger.Error($"Error getting message status, docGuid {message.DocGuid}, reseiver {message.Sender.Name} ", ex);
                LogMessagesHelper.LogCommunication(message.MessageGuid, null, ex.Message, null, false, true, logger);
            }
        }
    }
}
