using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Autofac;
using Grpc.Net.Client.Web;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Polly;
using static ED.Blobs.Blobs;
using static ED.Keystore.Keystore;

namespace ED.Domain
{
    public class DomainModule : EDModule
    {
        private Assembly domainAssembly = typeof(DomainModule).GetTypeInfo().Assembly;

        public DomainModule(
            IConfiguration configuration,
            IWebHostEnvironment environment)
            : base(configuration, environment)
        {
        }

        protected override void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            services.AddEntityFrameworkSqlServer()
                .AddDbContextFactory<DbContext>(
                    (serviceProvider, optionsBuilder) =>
                    {
                        optionsBuilder
                            .UseInternalServiceProvider(serviceProvider);

                        DomainOptions options = serviceProvider
                            .GetRequiredService<IOptions<DomainOptions>>().Value;

                        optionsBuilder.UseSqlServer(
                            options.GetConnectionString());

                        if (environment.IsDevelopment())
                        {
                            optionsBuilder.EnableSensitiveDataLogging(true);
                        }
                    },
                    ServiceLifetime.Singleton);

            services.Configure<DomainOptions>(configuration.GetSection("ED:Domain"));
            services.Configure<PdfOptions>(configuration.GetSection("ED:Pdf"));
            services.Configure<RegixOptions>(configuration.GetSection("ED:Regix"));

            var domainOptions = new DomainOptions();
            configuration.GetSection("ED:Domain").Bind(domainOptions);

            services.AddMediatR(this.domainAssembly);

            services.AddSingleton<IEncryptorFactory, EncryptorFactoryV2>();
            services.AddSingleton<EncryptorFactoryV1>();
            services.AddSingleton<EncryptorFactoryV2>();

            services.AddTransient<RegixServiceClient>();

            this.AddGrpcClient<KeystoreClient>(
                services,
                domainOptions.KeystoreServiceUrl,
                domainOptions.KeystoreServiceUseGrpcWeb);
            this.AddGrpcClient<BlobsClient>(
                services,
                domainOptions.BlobsServiceUrl,
                domainOptions.BlobsServiceUseGrpcWeb);

            services.AddHttpClient<TimestampServiceClient>(
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(
                        domainOptions.TimestampServiceUrl
                        ?? throw new Exception($"Missing setting {nameof(DomainOptions)}.{nameof(DomainOptions.TimestampServiceUrl)}")
                    );
                })
                .AddTransientHttpErrorPolicy(
                    p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

            services.AddTransient<BlobsServiceClient>();
            services.AddSingleton(
                (sp) =>
                    new RecyclableMemoryStreamManager(
                        blockSize: 128 * 1024, // 128 KB
                        largeBufferMultiple: 1 * 1024 * 1024, // 1 MB
                        maximumBufferSize: 20 * 1024 * 1024, // 20 MB
                        maximumSmallPoolFreeBytes: 512 * 1024 * 1024, // 128 MB
                        maximumLargePoolFreeBytes: 1 * 1024 * 1024 * 1024L)); // 1 GB
        }

        protected override void ConfigureAutofacServices(
            ContainerBuilder builder,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            builder.RegisterAssemblyTypes(this.domainAssembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(this.domainAssembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(this.domainAssembly)
                .Where(type =>
                    !type.GetTypeInfo().IsAbstract &&
                    type.GetInterfaces()
                        .Any(i => i.Equals(typeof(IEntityMapping))))
                .As<IEntityMapping>();

            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(AggregateRepository<>)).As(typeof(IAggregateRepository<>)).InstancePerLifetimeScope();
            builder.RegisterType<ProfileAggregateRepository>().As<IAggregateRepository<Profile>>().InstancePerLifetimeScope();
            builder.RegisterType<TemplateAggregateRepository>().As<IAggregateRepository<Template>>().InstancePerLifetimeScope();
            builder.RegisterType<MessageAggregateRepository>().As<IAggregateRepository<Message>>().InstancePerLifetimeScope();
            builder.RegisterType<RecipientGroupAggregateRepository>().As<IAggregateRepository<RecipientGroup>>().InstancePerLifetimeScope();
            builder.RegisterType<TargetGroupAggregateRepository>().As<IAggregateRepository<TargetGroup>>().InstancePerLifetimeScope();
            builder.RegisterType<TargetGroupProfileAggregateRepository>()
                .As<IAggregateRepository<TargetGroupProfile>>()
                .As<ITargetGroupProfileAggregateRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<AdminUserAggregateRepository>().As<IAggregateRepository<AdminUser>>().InstancePerLifetimeScope();
            builder.RegisterType<LoginAggregateRepository>()
                .As<LoginAggregateRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterType<LoginSecurityLevelNomsRepository>().As<ILoginSecurityLevelNomsRepository>().InstancePerLifetimeScope();
            builder.RegisterType<CountryNomsRepository>().As<ICountryNomsRepository>().InstancePerLifetimeScope();
        }

        private void AddGrpcClient<T>(
            IServiceCollection services,
            string? address,
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
                    () => new SocketsHttpHandler()
                    {
                        EnableMultipleHttp2Connections = true,
                        SslOptions = new()
                        {
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
                            RemoteCertificateValidationCallback = delegate
                            { return true; },
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                        }
                    });
            }
        }
    }
}
