using EDelivery.SEOS.DataContracts;
using EDelivery.SEOS.Models;
using log4net;

namespace EDelivery.SEOS.MessagesTypes
{
    public interface IMessage
    {
        MessageCreationResult Receive(Message requestMessage, ILog logger);
    }
}
