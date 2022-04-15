using System;
using System.Linq;
using System.Net;
using System.Text;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.EDeliveryAS4Node;
using EDelivery.SEOS.Models;

namespace EDelivery.SEOS.Utils
{
    public class As4Helper
    {
        public static SendMessageResult SendMessage(string xmlMessage, string eDeliveryAS4, string receiverAS4, string senderAS4)
        {
            try
            {
                using (var service = new EDeliveryAS4Node.BackendInterfaceClient())
                {
                    var message = CreateMessaging(eDeliveryAS4, receiverAS4, senderAS4);
                    var submitRequest = CreateSubmitRequest(Encoding.UTF8.GetBytes(xmlMessage));

                    var result = service.submitMessage(message, submitRequest);
                    var messageId = result.FirstOrDefault() ?? String.Empty;

                    return new SendMessageResult
                    {
                        ErrorMessage = String.Empty,
                        Status = DocumentStatusType.DS_TRY_SEND,
                        Request = messageId
                    };
                }
            }
            catch (WebException e1)
            {
                return new SendMessageResult
                {
                    ErrorMessage = e1.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    Request = String.Empty
                };
            }
            catch (System.ServiceModel.EndpointNotFoundException e2)
            {
                return new SendMessageResult
                {
                    ErrorMessage = e2.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    Request = String.Empty
                };
            }
            catch (System.ServiceModel.CommunicationException e3)
            {
                return new SendMessageResult
                {
                    ErrorMessage = e3.Message,
                    Status = DocumentStatusType.DS_SENT_FAILED,
                    Request = String.Empty
                };
            }
            catch (Exception ex)
            {
                return new SendMessageResult
                {
                    ErrorMessage = ex.Message,
                    Status = DocumentStatusType.DS_TRY_SEND,
                    Request = String.Empty
                };
            }
        }

        public static MsgData DownloadMessage(String msgId)
        {
            var msgData = new MsgData();
            var result = String.Empty;

            using (var service = new EDeliveryAS4Node.BackendInterfaceClient())
            {
                var messageId = CreateRetrieveMessageRequest(msgId);
                var messageRequest = new EDeliveryAS4Node.retrieveMessageResponse();

                var retrieveResult = service.retrieveMessage(messageId, out messageRequest);

                msgData.l1Timestamp = retrieveResult.UserMessage.MessageInfo.Timestamp.ToString("G");
                msgData.l1MessageId = retrieveResult.UserMessage.MessageInfo.MessageId;
                msgData.l1From = retrieveResult.UserMessage.PartyInfo.From.PartyId.Value;
                msgData.l1To = retrieveResult.UserMessage.PartyInfo.To.PartyId.Value;
                msgData.originalSender = retrieveResult.UserMessage.MessageProperties.FirstOrDefault(p => p.name.Equals("originalSender")).Value;
                msgData.finalRecepient = retrieveResult.UserMessage.MessageProperties.FirstOrDefault(p => p.name.Equals("finalRecipient")).Value;

                result = Encoding.UTF8.GetString(messageRequest.payload.FirstOrDefault().value);
                msgData.payload = result;
            }

            return msgData;
        }

        public static string[] ListPendingMessages()
        {
            var result = new string[0];

            using (var service = new EDeliveryAS4Node.BackendInterfaceClient())
            {
                result = service.listPendingMessages(String.Empty);
            }

            return result;
        }

        public static messageStatus? GetMessageStatus(string id)
        {
            using (var service = new BackendInterfaceClient())
            {
                var request = new statusRequest { messageID = id };
                return service.getStatus(request);
            }
        }

        public static errorResultImpl[] GetMessageErrors(string id)
        {
            using (var service = new BackendInterfaceClient())
            {
                var request = new getErrorsRequest { messageID = id };
                return service.getMessageErrors(request);
            }
        }

        public static void CheckSenderCoincide(string as4Node, string uic)
        {
            var as4NodeFromRegister = DatabaseQueries.GetAS4Node(uic);
            if (as4Node.ToLower() != as4NodeFromRegister.ToLower())
                throw new ApplicationException(String.Format(
                        Resources.Resource.SenderDiscrepancy,
                        as4Node,
                        as4NodeFromRegister));
        }

        public static void CheckReceiverCoincide(string as4Node, string uic)
        {
            var as4NodeFromRegister = DatabaseQueries.GetAS4Node(uic);
            if (as4Node.ToLower() != as4NodeFromRegister.ToLower())
                throw new ApplicationException(String.Format(
                        Resources.Resource.ReceiverDiscrepancy,
                        as4Node,
                        as4NodeFromRegister));
        }

        private static Messaging CreateMessaging(string eDeliveryAS4, string receiverAS4, string senderAS4)
        {
            var message = new EDeliveryAS4Node.Messaging();
            message.UserMessage = new EDeliveryAS4Node.UserMessage();
            message.UserMessage.PartyInfo = new EDeliveryAS4Node.PartyInfo();

            message.UserMessage.PartyInfo.To = new EDeliveryAS4Node.To();
            message.UserMessage.PartyInfo.To.Role = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/responder";
            message.UserMessage.PartyInfo.To.PartyId = new EDeliveryAS4Node.PartyId
            { Value = receiverAS4, type = "urn:oasis:names:tc:ebcore:partyid-type:unregistered" };

            message.UserMessage.PartyInfo.From = new EDeliveryAS4Node.From();
            message.UserMessage.PartyInfo.From.Role = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/initiator";
            message.UserMessage.PartyInfo.From.PartyId = new EDeliveryAS4Node.PartyId
            { Value = eDeliveryAS4, type = "urn:oasis:names:tc:ebcore:partyid-type:unregistered" };

            message.UserMessage.CollaborationInfo = new EDeliveryAS4Node.CollaborationInfo();
            message.UserMessage.CollaborationInfo.Service = new EDeliveryAS4Node.Service
            { type = "tc1", Value = "bdx:noprocess" };

            message.UserMessage.CollaborationInfo.Action = "TC1Leg1";

            var property = new EDeliveryAS4Node.Property
            { name = "MimeType", Value = "text/xml" };

            var partInfo = new EDeliveryAS4Node.PartInfo
            { href = "cid:message", PartProperties = new EDeliveryAS4Node.Property[] { property } };

            message.UserMessage.PayloadInfo = new EDeliveryAS4Node.PartInfo[] { partInfo };

            var from = new EDeliveryAS4Node.Property
            { name = "originalSender", Value = senderAS4 };

            var to = new EDeliveryAS4Node.Property
            { name = "finalRecipient", Value = receiverAS4 };

            message.UserMessage.MessageProperties = new EDeliveryAS4Node.Property[] { from, to };

            message.UserMessage.MessageInfo = new EDeliveryAS4Node.MessageInfo();

            return message;
        }

        private static EDeliveryAS4Node.submitRequest CreateSubmitRequest(byte[] message)
        {
            var submitRequest = new EDeliveryAS4Node.submitRequest();

            var payload = new EDeliveryAS4Node.LargePayloadType
            { value = message, payloadId = "cid:message", contentType = "text/xml" };

            submitRequest.payload = new EDeliveryAS4Node.LargePayloadType[] { payload };

            return submitRequest;
        }

        private static EDeliveryAS4Node.retrieveMessageRequest CreateRetrieveMessageRequest(string msgId)
        {
            return new EDeliveryAS4Node.retrieveMessageRequest { messageID = msgId };
        }
    }
}
