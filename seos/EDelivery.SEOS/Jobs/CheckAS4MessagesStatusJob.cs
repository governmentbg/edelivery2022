using System;
using System.IO;
using System.Xml.XPath;
using EDelivery.SEOS.Utils;
using EDelivery.SEOS.DatabaseAccess;
using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.MessagesRedirect;
using EDelivery.SEOS.MessagesSend;
using EDelivery.SEOS.MessagesTypes;
using EDelivery.SEOS.Models;
using log4net;

namespace EDelivery.SEOS.Jobs
{
    public class CheckAs4MessagesStatusJob : Job
    {
        public CheckAs4MessagesStatusJob(string name) : base(name)
        {
        }

        protected override void Execute()
        {
            try
            {
                var messages = DatabaseQueries.GetAs4SentMessageStatus();
                foreach (var msg in messages)
                {
                    var status = As4Helper.GetMessageStatus(msg.AS4Guid);
                    if (!status.HasValue)
                        continue;

                    if (status.Value == EDeliveryAS4Node.messageStatus.SEND_FAILURE ||
                        status.Value == EDeliveryAS4Node.messageStatus.SEND_ATTEMPT_FAILED)
                    {
                        SendErrorResponse(msg);
                        msg.SentResponse = true;
                    }

                    if (msg.Status != (int)status.Value)
                    {
                        msg.Status = (int)status.Value;
                        msg.DateStatusChange = DateTime.Now;
                    }
                }
                DatabaseQueries.UpdateAs4SentMessageStatus(messages);
            }
            catch (Exception ex)
            {
                logger.Error("Error in CheckAs4MessagesStatusJob: ", ex);
            }
        }

        private void SendErrorResponse(AS4SentMessagesStatus msg)
        {
            var sender = DatabaseQueries.GetRegisteredEntity(msg.SenderIdentifier);
            var receiver = DatabaseQueries.GetRegisteredEntity(msg.ReceiverIdentifier);
            if (sender == null || receiver == null)
                return;

            var settings = new MessageCreationSettings
            {
                MessageGuid = msg.MessageGuid,
                DocIdentity = null,
                Sender = MapperHelper.Mapping
                .Map<RegisteredEntity, EntityNodeType>(sender),
                Receiver = MapperHelper.Mapping
                .Map<RegisteredEntity, EntityNodeType>(receiver),
                RejectionReason = String.Empty,
                DocExpectCloseDateSpecified = false,
                DocExpectCloseDate = DateTime.MinValue
            };

            var errors = As4Helper.GetMessageErrors(msg.AS4Guid);
            if (errors.Length == 0)
                return;

            var response = MsgError.Create(settings,
                ErrorKindType.ERR_EXTERNAL,
                errors[0].errorDetail,
                logger);

            var properties = new SendMessageProperties
            {
                MessageGuid = Guid.Parse(settings.MessageGuid),
                MessageXml = response,
                Receiver = settings.Sender,
                ReceiverId = sender.Id,
                ReceiverServiceUrl = sender.ServiceUrl,
                Sender = settings.Receiver,
                SenderCertificateSN = receiver.CertificateSN
            };

            var submitHandler = SubmitMessageFactory.CreateInstance(
                properties.Receiver.Identifier,
                false,
                false);

            var result = submitHandler.Submit(properties,
                MessageType.MSG_Error,
                logger);

            if (!String.IsNullOrEmpty(result.Error))
                logger.Error($"CheckStatusErrorResponse AS4Guid {msg.AS4Guid}, " +
                    $"Message Guid {msg.MessageGuid} error: {result.Error}");
        }
    }
}
