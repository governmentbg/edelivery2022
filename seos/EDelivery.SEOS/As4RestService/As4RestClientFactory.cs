using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace EDelivery.SEOS.As4RestService
{
    public class As4RestClientFactory
    {
        private static readonly Lazy<IServiceProvider> As4RestProvider =
            new Lazy<IServiceProvider>(
                () =>
                {
                    var services = new ServiceCollection();

                    services
                        .AddHttpClient<As4RestClient>(
                            (client) =>
                            {
                                client.BaseAddress =
                                    new Uri(WebConfigurationManager.AppSettings["DomibusRestServiceUrl"]);

                                client.DefaultRequestHeaders.Add("Accept", "*/*");

                                var authentication = $"{WebConfigurationManager.AppSettings["DomibusRestServiceUser"]}:" +
                                                     $"{WebConfigurationManager.AppSettings["DomibusRestServicePassword"]}";

                                var base64Authentication = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authentication));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Authentication);
                            })
                        .ConfigurePrimaryHttpMessageHandler(
                            () =>
                            {
                                var handler = new HttpClientHandler();

                                handler.ServerCertificateCustomValidationCallback =
                                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                                return handler;
                            })
                        .AddTransientHttpErrorPolicy(
                            p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

                    return services.BuildServiceProvider();
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static As4RestClient CreateClient()
        {
            return As4RestProvider.Value.GetRequiredService<As4RestClient>();
        }
    }
}
