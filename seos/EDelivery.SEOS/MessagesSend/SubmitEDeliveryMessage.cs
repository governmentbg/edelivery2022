using System;
using EDelivery.SEOS.DBEntities;
using EDelivery.SEOS.DataContracts;
using log4net;

namespace EDelivery.SEOS.MessagesSend
{
    public class SubmitEDeliveryMessage : ISubmitMessage
    {
        public SubmitStatusRequestResult Submit(SendMessageProperties properties, MessageType type, ILog logger)
        {
            return new SubmitStatusRequestResult();
        }

        public SubmitStatusRequestResult Submit(SEOSMessage message, MessageType type, ILog logger)
        {
            var result = new SubmitStatusRequestResult();

            result.Status = DocumentStatusType.DS_WAIT_REGISTRATION;
            result.StatusResponse = new DocumentStatusResponseType()
            {
                DocRegStatus = DocumentStatusType.DS_WAIT_REGISTRATION,
                DocID = new DocumentIdentificationType()
                {
                    DocumentGUID = message.DocGuid.ToString("B"),
                    Item = new DocumentNumberType()
                    {
                        DocDate = message.DateCreated,
                        DocNumber = message.DocNumberInternal
                    }
                }
            };

            return result;
        }
    }
}
