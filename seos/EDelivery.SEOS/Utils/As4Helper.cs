using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using log4net;
using EDelivery.SEOS.As4RestService;
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
                using (var service = new WebServicePluginInterfaceClient())
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

        public static async Task DownloadMessages(
            string[] ids,
            ILog log,
            Action<MsgData> processMessage)
        {
            foreach (var id in ids)
            {
                try
                {
                    await DownloadMessage(
                        id,
                        log,
                        processMessage);
                }
                catch (Exception ex)
                {
                    log.Error($"Error processing downloaded message {id}: {ex.Message} {ex.InnerException?.Message}");
                }
            }
        }

        public static async Task DownloadMessage(
            string id,
            ILog log,
            Action<MsgData> processMessage)
        {
            try
            {
                var messageStatus = GetMessageStatus(id);
                if (messageStatus.HasValue && messageStatus.Value == EDeliveryAS4Node.messageStatus.DELETED)
                    return;

                var message =  await DownloadMessageSOAP(id);
                processMessage(message);
            }
            catch (Exception exSoap)
            {
                log.Error($"Error downloading SOAP message {id}: {exSoap.Message} {exSoap.InnerException?.Message}");

                try
                {
                    var message = await DownloadMessageREST(id);
                    processMessage(message);

                    await DeleteMessage(id);
                }
                catch (Exception exRest)
                {
                    log.Error($"Error downloading REST message {id}: {exRest.ToString()} {exRest.InnerException?.Message}");
                }
            }
        }

        public static async Task<MsgData> DownloadMessageSOAP(string id)
        {
            var msgData = new MsgData();

            using (var service = new WebServicePluginInterfaceClient())
            {
                var request =  new retrieveMessageRequest { messageID = id };

                var retrieveResult = await service.retrieveMessageAsync(request);

                msgData.l1Timestamp = retrieveResult.Messaging
                    .UserMessage
                    .MessageInfo
                    .Timestamp
                    .ToString("G");

                msgData.l1MessageId = retrieveResult.Messaging
                    .UserMessage
                    .MessageInfo
                    .MessageId;

                msgData.l1From = retrieveResult.Messaging
                    .UserMessage
                    .PartyInfo
                    .From
                    .PartyId.Value;

                msgData.l1To = retrieveResult.Messaging
                    .UserMessage
                    .PartyInfo
                    .To
                    .PartyId.Value;

                msgData.originalSender = retrieveResult.Messaging
                    .UserMessage
                    .MessageProperties
                    .FirstOrDefault(p => p.name.Equals("originalSender"))
                    .Value;

                msgData.finalRecepient = retrieveResult.Messaging
                    .UserMessage
                    .MessageProperties
                    .FirstOrDefault(p => p.name.Equals("finalRecipient"))
                    .Value;

                var content = Encoding.UTF8.GetString(
                    retrieveResult.retrieveMessageResponse.payload.FirstOrDefault().value);

                msgData.payload = content;

                return msgData;
            }
        }

        public static async Task<MsgData> DownloadMessageREST(string id)
        {
            var ns = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/";

            var msgData = new MsgData();

            var as4RestClient = As4RestClientFactory.CreateClient();

            var content = await as4RestClient.GetPayloadAsync(
                id,
                default(CancellationToken));

            var info = await as4RestClient.GetEnvelopeAsync(
                id,
                default(CancellationToken));

            var infoXml = new XmlDocument();
            infoXml.LoadXml(info);
            var manager = new XmlNamespaceManager(infoXml.NameTable);
            manager.AddNamespace("eb", ns);

            var timestampXml = infoXml.SelectSingleNode(
                "//eb:MessageInfo/eb:Timestamp",
                manager);
            msgData.l1Timestamp = timestampXml != null
                ? timestampXml.InnerText
                : String.Empty;

            var idXml = infoXml.SelectSingleNode(
                "//eb:MessageInfo/eb:MessageId", 
                manager);
            msgData.l1MessageId = idXml != null
                ? idXml.InnerText
                : String.Empty;

            var fromXml = infoXml.SelectSingleNode(
                "//eb:PartyInfo/eb:From/eb:PartyId", 
                manager);
            msgData.l1From = fromXml != null
                ? fromXml.InnerText
                : String.Empty;

            var toXml = infoXml.SelectSingleNode(
                "//eb:PartyInfo/eb:To/eb:PartyId", 
                manager);
            msgData.l1To = toXml != null
                ? toXml.InnerText
                : String.Empty;

            var originalSenderXml = infoXml.SelectSingleNode(
                "//eb:MessageProperties/eb:Property[@name='originalSender']", 
                manager);
            msgData.originalSender = originalSenderXml != null
                ? originalSenderXml.InnerText
                : String.Empty;

            var finalRecipientXml = infoXml.SelectSingleNode(
                "//eb:MessageProperties/eb:Property[@name='finalRecipient']", 
                manager);
            msgData.finalRecepient = finalRecipientXml != null
                ? finalRecipientXml.InnerText
                : String.Empty;

            msgData.payload = content;

            return msgData;
        }

        public static string[] ListPendingMessages()
        {
            var result = new string[0];

            using (var service = new WebServicePluginInterfaceClient())
            {
                var request = new listPendingMessagesRequest { };
                result = service.listPendingMessages(request);
            }

            return result;
        }

        public static messageStatus? GetMessageStatus(string id)
        {
            using (var service = new WebServicePluginInterfaceClient())
            {
                var request = new statusRequest { messageID = id };
                return service.getStatus(request);
            }
        }

        public static errorResultImpl[] GetMessageErrors(string id)
        {
            using (var service = new WebServicePluginInterfaceClient())
            {
                var request = new getErrorsRequest { messageID = id };
                return service.getMessageErrors(request);
            }
        }
        
        public static string GetRecepientUic(MsgData message)
        {
            using (var sr = new StringReader(message.payload))
            {
                var xdoc = new XPathDocument(sr);

                var identifier = xdoc.CreateNavigator().SelectSingleNode("//*[local-name()='Message']/*[local-name()='Header']/*[local-name()='Recipient']/*[local-name()='Identifier']");

                if (identifier == null)
                    throw new ApplicationException($"The recipient ID of the message {message.l1MessageId} could not be found");

                return identifier.Value;
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

        public static async Task<bool> DeleteMessage(string id)
        {
            var as4RestClient = As4RestClientFactory.CreateClient();

            var result = await as4RestClient.DeletePayloadAsync(
                id,
                default(CancellationToken));

            return result;
        }
    }
}
