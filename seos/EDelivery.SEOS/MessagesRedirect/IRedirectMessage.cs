using EDelivery.SEOS.Models;
using EDelivery.SEOS.DataContracts;
using log4net;

namespace EDelivery.SEOS.MessagesRedirect
{
    public interface IRedirectMessage
    {
        RedirectStatusRequestResult Redirect(string receiverUic, MsgData message, ILog logger);

        string CreateRedirectStatusResponse(RedirectStatusRequestResult statusResult, ILog logger);
    }
}
