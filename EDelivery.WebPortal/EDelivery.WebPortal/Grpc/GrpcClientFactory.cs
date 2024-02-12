using System;
using System.Configuration;
using System.Net.Http;
using System.Threading;

using Grpc.Net.Client.Web;

using Microsoft.Extensions.DependencyInjection;

using static ED.DomainServices.Authorization;
using static ED.DomainServices.Blobs.Blob;
using static ED.DomainServices.CodeMessages.CodeMessage;
using static ED.DomainServices.Journals.Journal;
using static ED.DomainServices.Messages.Message;
using static ED.DomainServices.Nomenclatures.Nomenclature;
using static ED.DomainServices.Profiles.Profile;
using static ED.DomainServices.Templates.Template;
using static ED.DomainServices.Tickets.Ticket;
using static ED.DomainServices.Translations.Translation;

namespace EDelivery.WebPortal.Grpc
{
    public static class GrpcClientFactory
    {
        private static readonly Lazy<IServiceProvider> GrpcServiceProvider =
            new Lazy<IServiceProvider>(
                () =>
                {
                    ServiceCollection services = new ServiceCollection();

                    string domainServicesUrl = ConfigurationManager.AppSettings["DomainServicesUrl"];
                    bool domainServicesUseGrpcWeb =
                        "True".Equals(
                            ConfigurationManager.AppSettings["DomainServicesUseGrpcWeb"],
                            StringComparison.InvariantCultureIgnoreCase);

                    AddGrpcClient<BlobClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<TemplateClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<NomenclatureClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<MessageClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<CodeMessageClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<ProfileClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<JournalClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<AuthorizationClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<TranslationClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);
                    AddGrpcClient<TicketClient>(services, domainServicesUrl, domainServicesUseGrpcWeb);

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
                });

            if (useGrpcWeb)
            {
                grpcHttpClientBuilder.ConfigurePrimaryHttpMessageHandler(
                    () => new GrpcWebHandler(new HttpClientHandler())
                    {
                        HttpVersion = new Version(1, 1)
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

        public static BlobClient CreateBlobClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<BlobClient>();
        }

        public static TemplateClient CreateTemplateClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<TemplateClient>();
        }

        public static NomenclatureClient CreateNomenclatureClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<NomenclatureClient>();
        }

        public static MessageClient CreateMessageClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<MessageClient>();
        }

        public static CodeMessageClient CreateCodeMessageClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<CodeMessageClient>();
        }

        public static ProfileClient CreateProfileClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<ProfileClient>();
        }

        public static JournalClient CreateJournalClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<JournalClient>();
        }

        public static AuthorizationClient CreateAuthorizationClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<AuthorizationClient>();
        }

        public static TranslationClient CreateTranslationClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<TranslationClient>();
        }

        public static TicketClient CreateTicketClient()
        {
            return GrpcServiceProvider.Value.GetRequiredService<TicketClient>();
        }
    }
}
