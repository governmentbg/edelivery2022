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
            try
            {
                var senderGuid = Guid.Parse(requestMessage.Header.Sender.GUID);
                var sender = DatabaseQueries.GetRegisteredEntity(senderGuid);
                if (sender == null)
                {
                    throw new ApplicationException(Resources.Resource.InvalidSender);
                }

                var docGuid = requestMessage.GetDocumentGuid();
                if (docGuid == Guid.Empty)
                {
                    throw new ApplicationException(Resources.Resource.ErrorDocumentIsNotFound);
                }

                var seosMessage = DatabaseQueries.GetReceivedMessageByDocId(
                    docGuid,
                    sender.Id);

                if (seosMessage == null)
                {
                    throw new ApplicationException(
                        ErrorsHelper.Describe(Resources.Resource.ErrorDocumentIsNotFound, docGuid));
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
            catch (Exception ex)
            {
                var settings = MapperHelper.Mapping
                    .Map<Message, MessageCreationSettings>(requestMessage);
                var response = MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    ErrorsHelper.Describe(Resources.Resource.ErrorInReceiver, ex),
                    logger);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_Error
                };
            }
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
