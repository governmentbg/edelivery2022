using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using System;
using log4net;

namespace EDelivery.SEOS.MessagesTypes
{
    public class MsgError : IMessage
    {
        public MessageCreationResult Receive(Message requestMessage, ILog logger)
        {
            var item = requestMessage.Body.Item as ErrorMessageType;
            if (String.IsNullOrEmpty(item.MessageGUID))
                return MessageCreationResult.Empty;

            var seosMessage = DatabaseQueries.GetMessage(
                Guid.Parse(item.MessageGUID), false, false);
            if (seosMessage == null)
                return MessageCreationResult.Empty;

            var result = new SubmitStatusRequestResult
            {
                Error = item.ErrorDescription,
                Status = DocumentStatusType.DS_SENT_FAILED,
                StatusResponse = null
            };
            DatabaseQueries.ApplySentDocumentStatus(seosMessage.Id, result);

            logger.Error($"Error in message {item.MessageGUID} {item.ErrorDescription}");

            return MessageCreationResult.Empty;
        }

        public static string Create(
            MessageCreationSettings settings,
            ErrorKindType errorType, 
            string errorMessage, 
            ILog logger)
        {
            try
            {
                if (settings == null)
                    throw new ApplicationException(Resources.Resource.InvalidXmlDocument);

                logger.Info($"GenerateErrorResponse for message GUID {settings.MessageGuid}, " +
                    $"errorType {errorType}, errorMessage {errorMessage}");
                if (errorMessage == Resources.Resource.InvalidReceiver)
                {
                    //in case the receiver is not institution supported by us, then use Edelivyer guid
                    var eDeliveryEntity = DatabaseQueries.GetEDeliveryEntity();
                    settings.Receiver = MapperHelper.Mapping
                        .Map<RegisteredEntity, EntityNodeType>(eDeliveryEntity);
                }

                var errorBody = new ErrorMessageType()
                {
                    ErrorType = errorType,
                    ErrorDescription = errorMessage,
                    MessageGUID = settings.MessageGuid
                };

                var message = SignedXmlHelper.WrapAndSignMessage(settings.Receiver, settings.Sender, MessageType.MSG_Error, errorBody);
                logger.Info($"Error message response: {message}");
                return message;
            }
            catch (Exception ex)
            {
                logger.Error("Error generating error response", ex);
                throw new Exception(errorMessage);
            }
        }
    }
}
