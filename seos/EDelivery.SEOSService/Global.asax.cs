using System;
using System.Web.Configuration;
using EDelivery.SEOS.Jobs;
using EDelivery.SEOS.Utils;
using log4net;

namespace EDelivery.SEOSService
{
    public class Global : System.Web.HttpApplication
    {
        private static ILog logger = LogManager.GetLogger("Global");
        protected void Application_Start(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.Expect100Continue = true;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(SEOSService.ValidateServerCertificate);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls  | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Ssl3;

            log4net.Config.XmlConfigurator.Configure();

            MapperHelper.Initialize();

            try
            {
                var refreshJobIntervalMinutes = Int32.Parse(WebConfigurationManager
                    .AppSettings["SEOS.RefreshEntriesIntervalMinutes"] ?? "5");

                var retryingSendIntervalMinutes = Int32.Parse(WebConfigurationManager
                    .AppSettings["SEOS.RetryingSendIntervalMinutes"] ?? "5");

                var processAs4JobIntervalMinutes = Int32.Parse(WebConfigurationManager
                    .AppSettings["SEOS.ProcessAS4IntervalMinutes"] ?? "5");

                var checkAs4StatusJobIntervalMinutes = Int32.Parse(WebConfigurationManager
                    .AppSettings["SEOS.CheckAS4StatusIntervalMinutes"] ?? "5");

                var refreshJob = new RefreshRegisteredEntitiesJob("RefreshRegisteredEntitiesJob");
                refreshJob.Start(TimeSpan.FromMinutes(refreshJobIntervalMinutes));
                logger.Info("RefreshRegestryJob successfuly started");

                var resendJob = new RetryingSendDocumentRequestJob("RetryingSendDocumentRequestJob");
                resendJob.Start(TimeSpan.FromMinutes(retryingSendIntervalMinutes));
                logger.Info("RetryingSendDocumentRequestJob successfuly started");

                var as4MessagesJob = new ProcessAs4MessagesJob("ProcessAs4MessagesJob");
                as4MessagesJob.Start(TimeSpan.FromMinutes(processAs4JobIntervalMinutes));
                logger.Info("ProcessAs4MessagesJob successfuly started");

                var as4StatusJob = new CheckAs4MessagesStatusJob("CheckAS4MessagesStatusJob");
                as4StatusJob.Start(TimeSpan.FromMinutes(checkAs4StatusJobIntervalMinutes));
                logger.Info("ProcessAs4MessagesJob successfuly started");
            }
            catch (Exception ex)
            {
                logger.Error("Error in InitializeRefreshRegestryTask", ex);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var error = Server.GetLastError();
            logger.Error("Application error", error);
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}