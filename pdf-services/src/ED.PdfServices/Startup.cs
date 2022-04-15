using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.IO;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;

[assembly: OwinStartup(typeof(ED.PdfServices.Startup))]

namespace ED.PdfServices
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var mainLogger = SerilogExtensions.SetupSerilog();
            var logger = mainLogger.WithSourceContext<Startup>();

            try
            {
                var builder = new ContainerBuilder();

                ConfigureServices(builder, mainLogger);

                IContainer container = builder.Build();

                HttpConfiguration config = new HttpConfiguration();

                ConfigureWebApi(config, container);

                app.Use(async (owinContext, next) =>
                {
                    long start = Stopwatch.GetTimestamp();
                    await next();
                    long stop = Stopwatch.GetTimestamp();
                    double ms = ((double)(stop - start) / Stopwatch.Frequency) * 1000;
                    logger.Information($"Finished {owinContext.Request.Method} {owinContext.Request.Path} in {ms:#}ms with StatusCode {owinContext.Response.StatusCode}");
                });
                app.SetLoggerFactory(new SerilogOwinLoggerFactory());
                app.UseAutofacMiddleware(container);
                app.UseAutofacWebApi(config);
                app.UseWebApi(config);
                app.Run((ctx) =>
                {
                    ctx.Response.StatusCode = 404;
                    return Task.CompletedTask;
                });

                logger.Information("Application started.");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Application failed");
                Serilog.Log.CloseAndFlush();
                throw;
            }
        }

        private static void ConfigureServices(ContainerBuilder builder, Serilog.ILogger mainLogger)
        {
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).InstancePerRequest();
            builder.RegisterType<WebApiExceptionLogger>().As<IExceptionLogger>().SingleInstance();
            builder.RegisterInstance(new SerilogLoggerFactory(mainLogger))
                .As<Microsoft.Extensions.Logging.ILoggerFactory>();
            builder.RegisterGeneric(typeof(OpenGenericLoggerHelper<>))
                .As(typeof(Microsoft.Extensions.Logging.ILogger<>));
            builder.RegisterInstance(
                new RecyclableMemoryStreamManager(
                    blockSize: 128 * 1024, // 128 KB
                    largeBufferMultiple: 1 * 1024 * 1024, // 1 MB
                    maximumBufferSize: 20 * 1024 * 1024, // 20 MB
                    maximumSmallPoolFreeBytes: 512 * 1024 * 1024, // 128 MB
                    maximumLargePoolFreeBytes: 1 * 1024 * 1024 * 1024L)); // 1 GB
        }

        private static void ConfigureWebApi(HttpConfiguration config, IContainer container)
        {
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
#if DEBUG
                Formatting = Formatting.Indented,
#else
                Formatting = Formatting.None,
#endif
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;

            config.MapHttpAttributeRoutes();
        }
    }
}
