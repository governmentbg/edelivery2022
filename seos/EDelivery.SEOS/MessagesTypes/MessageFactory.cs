using EDelivery.SEOS.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDelivery.SEOS.MessagesTypes
{
    public class MessageFactory
    {
        public static IMessage CreateInstance(MessageType type)
        {
            switch (type)
            {
                case MessageType.MSG_DocumentRegistrationRequest:
                    return new MsgDocumentRegistrationRequest();
                case MessageType.MSG_DocumentStatusRequest:
                    return new MsgDocumentStatusRequest();
                case MessageType.MSG_DocumentStatusResponse:
                    return new MsgDocumentStatusResponse();
                case MessageType.MSG_Error:
                    return new MsgError();
                default:
                    return null;
            }
        }
    }
}
