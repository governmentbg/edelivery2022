using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EDelivery.WebPortal.Utils;
using log4net;

using Microsoft.Owin;

[assembly: OwinStartupAttribute(typeof(EDelivery.WebPortal.Startup))]
namespace EDelivery.WebPortal
{
    public class MvcApplication : HttpApplication
    {
        private static ILog logger = LogManager.GetLogger("Global");

        protected void Application_Start()
        {
            var jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            jsonFormatter.UseDataContractJsonSerializer = true;
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(jsonFormatter);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            this.InitLogger();
            RegixInfoClient.RegixConfiguration.Init();

            //disable X-AspNetMvc-Version Header
            MvcHandler.DisableMvcResponseHeader = true;
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = false;
            System.Web.Helpers.AntiForgeryConfig.RequireSsl = true;
        }

        protected void Application_End()
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Get the exception object.
            Exception exc = Server.GetLastError();
            logger.Warn("Application error", exc);
            ElmahLogger.Instance.Error(exc, "Application error");
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var httpContext = ((HttpApplication)sender).Context;

            //remove the server header
            httpContext.Response.Headers.Remove("Server");

            CultureHelper.ApplyCulture(httpContext);
        }

        private void InitLogger()
        {
            //init elmah logger
            ElmahLogger.Instance.Info("EDelivery WebPortal is starting...");
            //init log4net logger
            var path = ConfigurationManager.AppSettings["LogConfigurationPath"] ?? "log4net.config";
            if (!Path.IsPathRooted(path))
            {
                var assemblyPath = HostingEnvironment.MapPath("/");
                path = Path.Combine(assemblyPath, path);
            }

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            LogManager.GetLogger("Global").Debug("Logging is initialized!");
        }
    }
}
