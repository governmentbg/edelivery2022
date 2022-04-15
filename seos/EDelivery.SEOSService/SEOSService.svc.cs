using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using EDelivery.SEOS.MessagesReceive;
using log4net;

namespace EDelivery.SEOSService
{
    public class SEOSService : SEOS.EGovEndpoint.IEGovService
    {
        private static ILog logger = LogManager.GetLogger("SEOSReceiveLogger");

        public SEOSService()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public string Submit(string request)
        {
            var receiveHandler = new ReceiveSeosMessage();
            return receiveHandler.Submit(request, logger);
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
