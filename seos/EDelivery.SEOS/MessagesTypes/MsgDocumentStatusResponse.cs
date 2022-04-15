using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using System;
using log4net;

namespace EDelivery.SEOS.MessagesTypes
{
    public class MsgDocumentStatusResponse: IMessage
    {
        public MessageCreationResult Receive(Message requestMessage, ILog logger)
        {
            var docGuid = requestMessage.GetDocumentGuid();
            if (docGuid == Guid.Empty)
            {
                var settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(requestMessage);
                var response = MsgError.Create(settings,
                        ErrorKindType.ERR_EXTERNAL,
                        Resources.Resource.ErrorDocumentIsNotFound,
                        logger);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_Error
                };
            }

            var seosMessage = DatabaseQueries.GetMessageByDocId(docGuid);

            if (seosMessage == null)
            {
                var settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(requestMessage);
                var response = MsgError.Create(settings,
                            ErrorKindType.ERR_EXTERNAL,
                            ErrorsHelper.Describe(Resources.Resource.ErrorDocumentIsNotFound, docGuid),
                            logger);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_Error
                };
            }

            var item = requestMessage.Body.Item as DocumentStatusResponseType;
            var result = new SubmitStatusRequestResult
            {
                Error = String.Empty,
                Status = item != null
                ? item.DocRegStatus
                : DocumentStatusType.DS_NOT_FOUND,
                StatusResponse = item
            };
            DatabaseQueries.ApplySentDocumentStatus(seosMessage.Id, result);

            return MessageCreationResult.Empty;
        }

        public static string Create(
            MessageCreationSettings settings,
            DocumentStatusType statusType)
        {
            if (settings.DocIdentity == null ||
                settings.Sender == null ||
                settings.Receiver == null)
                return String.Empty;

            var responseBody = new DocumentStatusResponseType
            {
                DocID = settings.DocIdentity,
                DocRegStatus = statusType,
                RejectionReason = settings.RejectionReason,
                DocExpectCloseDateSpecified = settings.DocExpectCloseDateSpecified,
                DocExpectCloseDate = settings.DocExpectCloseDate
            };

            return SignedXmlHelper.WrapAndSignMessage(settings.Receiver, settings.Sender,
                MessageType.MSG_DocumentStatusResponse, responseBody);
        }
    }
}
