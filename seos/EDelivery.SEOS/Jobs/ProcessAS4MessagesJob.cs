using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.XPath;
using EDelivery.SEOS.Utils;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.MessagesRedirect;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.Models;

namespace EDelivery.SEOS.Jobs
{
    public class ProcessAs4MessagesJob : Job
    {
        public ProcessAs4MessagesJob(string name) : base(name)
        {
        }

        protected override void Execute()
        {
            try
            {
                var messages = As4Helper.ListPendingMessages();

                Task.Run(async() => await As4Helper.DownloadMessages(
                    messages,
                    logger,
                    this.ProcessPendingMessage));
            }
            catch (Exception ex)
            {
                logger.Error("Error in ProcessAs4MessagesJob download AS4 messages: ", ex);
            }
        }

        private void ProcessPendingMessage(MsgData message)
        {
            var uic = GetRecepientUic(message);

            var redirectHandler = RedirectMessageFactory.CreateInstance(
                uic, 
                message.originalSender);

            if (redirectHandler == null)
                return;

            logger.Info($"Redirect AS4 message type: {redirectHandler.GetType().Name}, " +
                $"Uic: {uic}, AS4Guid: {message.l1MessageId}");

            var redirectResult = redirectHandler.Redirect(uic,
                message,
                logger);

            var response = redirectHandler.CreateRedirectStatusResponse(
                redirectResult, logger);
            if (String.IsNullOrEmpty(response))
                return;

            var properties = MapperHelper.Mapping
                .Map<RedirectStatusRequestResult, SendMessageProperties>(redirectResult);
            properties.MessageXml = response;

            var submitHandler = SubmitMessageFactory.CreateInstance(
                redirectResult.Sender?.Identifier,
                    false,
                    false);

            var result = submitHandler.Submit(properties,
                redirectResult.MsgType,
                logger);

            if (!String.IsNullOrEmpty(result.Error))
                logger.Error($"RedirectResponse AS4Guid {message.l1MessageId}, " +
                    $"Message Guid {redirectResult.MessageGuid} error: {result.Error}");
        }

        private string GetRecepientUic(MsgData message)
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
    }
}
