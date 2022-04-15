using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesAttachments;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOS.MessagesTypes
{
    public class MsgDocumentRegistrationRequest : IMessage
    {
        public MessageCreationResult Receive(Message requestMessage, ILog logger)
        {
            try
            {
                var parentMessageId = GetParentMessageId(requestMessage);
                var eSubjectId = DatabaseQueries.GetElectronicSubjectIdByIdentifier(
                    requestMessage.Header.Recipient.Identifier);
                if (!eSubjectId.HasValue)
                {
                    throw new ApplicationException(Resources.Resource.MissingIdentityForReceiverEIK);
                }

                var docGuid = requestMessage.GetDocumentGuid();
                var savedMessage = DatabaseQueries.GetMessageByDocId(docGuid);
                if (savedMessage != null)
                {
                    savedMessage.Status = savedMessage.Status == (int)DocumentStatusType.DS_WAIT_REGISTRATION
                    ? (int)DocumentStatusType.DS_ALREADY_RECEIVED
                    : savedMessage.Status;
                }
                else
                {
                    savedMessage = DatabaseQueries.CreateReceiveMessage(
                        requestMessage, eSubjectId.Value, parentMessageId);
                }

                Task.Run(async () => await AttachDocument.SendDocumentsForMalwareScan(
                    savedMessage.Attachments.ToList(), logger));

                var settings = MapperHelper.Mapping.Map<SEOSMessage, MessageCreationSettings>(savedMessage);
                settings.Sender = requestMessage.Header.Sender;
                settings.Receiver = requestMessage.Header.Recipient;
                var message = MsgDocumentStatusResponse.Create(
                    settings,
                    (DocumentStatusType)savedMessage.Status);

                return new MessageCreationResult
                {
                    Result = message,
                    Type = MessageType.MSG_DocumentStatusResponse
                };
            }
            catch (Exception ex)
            {
                var settings = MapperHelper.Mapping.Map<Message, MessageCreationSettings>(requestMessage);
                var response = MsgError.Create(settings,
                    ErrorKindType.ERR_INTERNAL,
                    ErrorsHelper.Describe(Resources.Resource.ErrorInReceiver, ex),
                    logger);

                SaveInRawFormatHelper.SaveRequest(
                    requestMessage.SerializeMessage().OuterXml, 
                    String.Empty, 
                    settings != null ? settings.MessageGuid : String.Empty, 
                    logger);

                return new MessageCreationResult
                {
                    Result = response,
                    Type = MessageType.MSG_Error
                };
            }
        }

        public static string Create(SEOSMessage message, ILog logger)
        {
            logger.Info($"Create RegistrationRequest for docId {message.DocGuid}, receiver {message.Sender.Name}");

            var regRequest = new DocumentRegistrationRequestType()
            {
                Document = new DocumentType()
                {
                    DocAbout = message.DocAbout,
                    DocAttachmentList = MapperHelper.Mapping
                    .Map<ICollection<SEOSMessageAttachment>, ICollection<AttachmentFileType>>(message.Attachments)
                    .ToArray(),
                    DocCorrespondentList = MapperHelper.Mapping
                    .Map<ICollection<SEOSMessageCorespondent>, ICollection<CorrespondentType>>(message.Corespondents)
                    .ToArray(),
                    DocAttentionTo = message.DocAttentionTo,
                    DocComment = message.DocComment,
                    DocID = new DocumentIdentificationType()
                    {
                        DocumentGUID = message.DocGuid.ToString("B"),
                        Item = new DocumentNumberType()
                        {
                            DocDate = message.DateCreated,
                            DocNumber = message.DocNumberInternal + (String.IsNullOrWhiteSpace(message.DocReferenceNumber) ?
                                                                        string.Empty :
                                                                        $" (Рег.индекс в АИС на изпращача - {message.DocReferenceNumber})")
                        }
                    },
                    DocKind = message.DocKind
                },
                Comment = message.Corespondents.First().Name
            };

            regRequest.Document.DocParentID = regRequest.Document.DocID;
            if (message.DocReqDateClose != null)
            {
                regRequest.Document.DocReqDateClose = message.DocReqDateClose.Value;
            }

            var receiver = new EntityNodeType()
            {
                AdministrativeBodyName = message.Receiver.Name,
                GUID = message.Receiver.UniqueId.ToString("B"),
                Identifier = message.Receiver.EIK
            };

            var sender = new EntityNodeType()
            {
                AdministrativeBodyName = message.Sender.Name,
                GUID = message.Sender.UniqueId.ToString("B"),
                Identifier = message.Sender.EIK
            };

            return SignedXmlHelper.WrapAndSignMessage(sender, receiver, 
                MessageType.MSG_DocumentRegistrationRequest, regRequest, message.MessageGuid);
        }

        private int? GetParentMessageId(Message requestMessage)
        {
            var messageBody = (requestMessage.Body.Item as DocumentRegistrationRequestType);

            if (string.IsNullOrEmpty(messageBody.Document.DocParentID?.DocumentGUID))
                return null;

            var parDocGuid = Guid.Parse(messageBody.Document.DocParentID.DocumentGUID);
            var parentMessage = DatabaseQueries.GetMessage(parDocGuid, true, false);

            return parentMessage != null
                ? parentMessage.Id
                : (int?)null;
        }
    }
}
