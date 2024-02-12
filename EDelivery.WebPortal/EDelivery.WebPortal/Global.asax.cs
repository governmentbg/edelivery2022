using System;
using System.Configuration;
using System.IO;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EDelivery.WebPortal.Utils;
using Elmah;
using Grpc.Core;
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
            JsonMediaTypeFormatter jsonFormatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            jsonFormatter.UseDataContractJsonSerializer = true;
            GlobalConfiguration.Configuration.Formatters.Clear();
            GlobalConfiguration.Configuration.Formatters.Add(jsonFormatter);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            this.InitLogger();

            //disable X-AspNetMvc-Version Header
            MvcHandler.DisableMvcResponseHeader = true;
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = false;
            System.Web.Helpers.AntiForgeryConfig.RequireSsl = true;
        }

        protected void Application_End()
        {
        }

        protected void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            if (e.Exception.GetBaseException() is RpcException rpcException
                && rpcException.StatusCode == StatusCode.Cancelled)
            {
                e.Dismiss();
                return;
            }

            logger.Warn("Application error", e.Exception);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpContext httpContext = ((HttpApplication)sender).Context;

            //remove the server header
            httpContext.Response.Headers.Remove("Server");

            CultureHelper.ApplyCulture(httpContext);
        }

        private void InitLogger()
        {
            //init elmah logger
            ElmahLogger.Instance.Error("EDelivery WebPortal is starting...");

            //init log4net logger
            string path = ConfigurationManager.AppSettings["LogConfigurationPath"] ?? "log4net.config";
            if (!Path.IsPathRooted(path))
            {
                string assemblyPath = HostingEnvironment.MapPath("/");
                path = Path.Combine(assemblyPath, path);
            }

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            LogManager.GetLogger("Global").Debug("Logging is initialized!");
        }
    }
}
