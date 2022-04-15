using System;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.MessagesReceive;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOS.MessagesRedirect
{
    public class RedirectFromAs4ToEDelivery : IRedirectMessage
    {
        public RedirectStatusRequestResult Redirect(string receiverUic, MsgData message, ILog logger)
        {
            var receiveHandler = new ReceiveAs4Message();
            var response = receiveHandler.Receive(message, logger);
            if (String.IsNullOrEmpty(response))
                return new RedirectStatusRequestResult();

            var msgResponse = response.DeserializeToMessage();

            try
            {
                As4Helper.CheckSenderCoincide(message.l1From, msgResponse.Header.Recipient.Identifier);

                return new RedirectStatusRequestResult
                {
                    Error = String.Empty,
                    Status = DocumentStatusType.DS_WAIT_REGISTRATION,
                    StatusResponse = null,
                    StatusResponseMessage = response,
                    MessageGuid = msgResponse.GetMessageGuid(),
                    MsgType = msgResponse.Header.MessageType,
                    DocIdentity = msgResponse.GetDocIdentity(),
                    Sender = msgResponse.Header.Recipient,
                    Receiver = msgResponse.Header.Sender
                };
            }
            catch (Exception ex)
            {
                return new RedirectStatusRequestResult
                {
                    Error = ex.Message,
                    Status = DocumentStatusType.DS_SENT_FAILED,
                    StatusResponse = null,
                    MessageGuid = msgResponse.GetMessageGuid(),
                    MsgType = msgResponse.Header.MessageType,
                    DocIdentity = msgResponse.GetDocIdentity(),
                    Sender = msgResponse.Header.Recipient,
                    Receiver = msgResponse.Header.Sender
                };
            }
        }

        public string CreateRedirectStatusResponse(
            RedirectStatusRequestResult statusResult,
            ILog logger)
        {
            if (statusResult.Sender == null || statusResult.Receiver == null)
                return String.Empty;

            var settings = new MessageCreationSettings
            {
                MessageGuid = statusResult.MessageGuid.ToString("B"),
                DocIdentity = statusResult.DocIdentity,
                Sender = statusResult.Sender,
                Receiver = statusResult.Receiver,
                RejectionReason = String.Empty,
                DocExpectCloseDateSpecified = false,
                DocExpectCloseDate = DateTime.MinValue
            };

            if (!String.IsNullOrEmpty(statusResult.Error))
            {
                return MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    statusResult.Error,
                    logger);
            }

            if (!String.IsNullOrEmpty(statusResult.StatusResponseMessage))
            {
                return statusResult.StatusResponseMessage;
            }

            return String.Empty;
        }
    }
}
