using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.DBEntities;
using log4net;

namespace EDelivery.SEOS.MessagesSend
{
    public interface ISubmitMessage
    {
        SubmitStatusRequestResult Submit(SendMessageProperties properties, MessageType type, ILog logger);

        SubmitStatusRequestResult Submit(SEOSMessage properties, MessageType type, ILog logger);
    }
}
