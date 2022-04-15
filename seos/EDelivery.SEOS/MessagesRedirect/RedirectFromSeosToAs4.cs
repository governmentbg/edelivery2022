using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Models;
using EDelivery.SEOS.Utils;
using System;
using log4net;

namespace EDelivery.SEOS.MessagesRedirect
{
    public class RedirectFromSeosToAs4
    {
        public string Redirect(Message message, string request, ILog logger)
        {
            var properties = MapperHelper.Mapping
                .Map<Message, SendMessageProperties>(message);
            properties.MessageXml = request;

            var submitHandler = SubmitMessageFactory.CreateInstance(
                properties.Receiver.Identifier,
                    false,
                    false);

            var result = submitHandler.Submit(properties,
                message.Header.MessageType,
                logger);

            var settings = MapperHelper.Mapping
                .Map<Message, MessageCreationSettings>(message);
            return CreateRedirectStatusResponse(result,
                settings,
                message.Header.MessageType,
                logger);
        }

        private string CreateRedirectStatusResponse(
            SubmitStatusRequestResult statusResult,
            MessageCreationSettings settings,
            MessageType messageType,
            ILog logger)
        {
            if (!String.IsNullOrEmpty(statusResult.Error))
            {
                return MsgError.Create(settings,
                    ErrorKindType.ERR_EXTERNAL,
                    statusResult.Error,
                    logger);
            }

            if (statusResult.StatusResponse != null)
            {
                return SignedXmlHelper.WrapAndSignMessage(settings.Receiver, settings.Sender,
                    MessageType.MSG_DocumentStatusResponse, statusResult.StatusResponse);
            }

            if (messageType == MessageType.MSG_DocumentRegistrationRequest ||
                messageType == MessageType.MSG_DocumentStatusRequest)
            {
                //Fake response
                return MsgDocumentStatusResponse.Create(
                    settings,
                    DocumentStatusType.DS_WAIT_REGISTRATION);
            }

            return String.Empty;
        }
    }
}
