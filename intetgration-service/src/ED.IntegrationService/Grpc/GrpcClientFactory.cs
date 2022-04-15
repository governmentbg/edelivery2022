using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.DependencyInjection;
using static ED.Blobs.Blobs;
using static ED.DomainServices.IntegrationService.IntegrationService;

namespace ED.IntegrationService
{
    public static class GrpcClientFactory
    {
        private static readonly Lazy<IServiceProvider> GrpcServiceProvider =
            new Lazy<IServiceProvider>(
                () =>
                {
                    var services = new ServiceCollection();

                    string domainServicesUrl = ConfigurationManager.AppSettings["DomainServicesUrl"];
                    bool domainServicesUseGrpcWeb =
                        "True".Equals(
                            ConfigurationManager.AppSettings["DomainServicesUseGrpcWeb"],
                            StringComparison.InvariantCultureIgnoreCase);

                    AddGrpcClient<IntegrationServiceClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);

                    string blobServicesUrl = ConfigurationManager.AppSettings["BlobsServiceUrl"];
                    bool blobServicesUseGrpcWeb =
                        "True".Equals(
                            ConfigurationManager.AppSettings["BlobsServiceUseGrpcWeb"],
                            StringComparison.InvariantCultureIgnoreCase);

                    AddGrpcClient<BlobsClient>(services, blobServicesUrl, blobServicesUseGrpcWeb);

                    return services.BuildServiceProvider();
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private static void AddGrpcClient<T>(
            IServiceCollection services,
            string address,
            bool useGrpcWeb)
            where T : class
        {
            address = address ?? throw new Exception($"Missing address for GrpcClient {typeof(T).Name}");

            IHttpClientBuilder grpcHttpClientBuilder = services.AddGrpcClient<T>(
                (options) =>
                {
                    options.Address = new Uri(address);
                    options.ChannelOptionsActions.Add(
                        (opts) =>
                        {
                            int maxReceiveMessageSize = int.Parse(
                                ConfigurationManager.AppSettings["GrpcClientMaxReceiveMessageSize"] ?? "50");

                            opts.MaxReceiveMessageSize = maxReceiveMessageSize * 1024 * 1024;
                        });
                });

            if (useGrpcWeb)
            {
                grpcHttpClientBuilder.ConfigurePrimaryHttpMessageHandler(
                    () => new GrpcWebHandler(new HttpClientHandler())
                    {
                        HttpVersion = new Version(1, 1),
                    });
            }
            else
            {
                grpcHttpClientBuilder.ConfigurePrimaryHttpMessageHandler(
                    () => new WinHttpHandler()
                    {
                        EnableMultipleHttp2Connections = true,
                        ServerCertificateValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    });
            }
        }

        public static IntegrationServiceClient CreateIntegrationServiceClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<IntegrationServiceClient>();
        }

        public static BlobsClient CreateBlobsClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<BlobsClient>();
        }
    }
}
