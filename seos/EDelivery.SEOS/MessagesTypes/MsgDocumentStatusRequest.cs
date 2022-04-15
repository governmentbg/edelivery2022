using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using System;
using log4net;

namespace EDelivery.SEOS.MessagesTypes
{
    public class MsgDocumentStatusRequest: IMessage
    {
        public MessageCreationResult Receive(Message requestMessage, ILog logger)
        {
            var receiverGuid = Guid.Parse(requestMessage.Header.Recipient.GUID);
            var recipient = DatabaseQueries.GetRegisteredEntity(receiverGuid);
            if (recipient == null)
            {
                var settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(requestMessage);
                var response = MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    Resources.Resource.InvalidReceiver,
                    logger);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_Error
                };
            }

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

            var seosMessage = DatabaseQueries.GetReceivedMessageByDocId(docGuid, recipient.Id);

            if (seosMessage == null)
            {
                var settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(requestMessage);
                var response = MsgDocumentStatusResponse.Create(
                    settings,
                    DocumentStatusType.DS_NOT_FOUND);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_DocumentStatusResponse
                };
            }
            else
            {
                var settings = MapperHelper.Mapping.Map<SEOSMessage, MessageCreationSettings>(seosMessage);
                settings.Sender = requestMessage.Header.Sender;
                settings.Receiver = requestMessage.Header.Recipient;
                var response = MsgDocumentStatusResponse.Create(
                    settings,
                    (DocumentStatusType)seosMessage.Status);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_DocumentStatusResponse
                };
            }
        }

        public static string Create(
            MessageCreationSettings settings)
        {
            var requestBody = new DocumentStatusRequestType
            {
                DocID = settings.DocIdentity,
            };

            return SignedXmlHelper.WrapAndSignMessage( settings.Sender, settings.Receiver,
                MessageType.MSG_DocumentStatusRequest, requestBody);
        }
    }
}
