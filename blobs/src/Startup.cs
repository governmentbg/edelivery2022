using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Polly;
using Serilog;
using static ED.DomainServices.Authorization;
using static ED.Keystore.Keystore;

namespace ED.Blobs
{
    public class Startup
    {
        private BlobsOptions BlobsOptions { get; init; }
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
            this.BlobsOptions = new BlobsOptions();
            configuration.GetSection("ED:Blobs").Bind(this.BlobsOptions);
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<BlobsOptions>(this.Configuration.GetSection("ED:Blobs"));
            services.Configure<DataOptions>(this.Configuration.GetSection("ED:Data"));

            services.AddSingleton<IDataProtector>(
                (_) =>
                {
                    string? sharedSecretDPKey = this.BlobsOptions.SharedSecretDPKey;
                    IDataProtector dataProtector;
                    if (!string.IsNullOrEmpty(sharedSecretDPKey))
                    {
                        dataProtector = new SharedSecretDataProtector(sharedSecretDPKey);
                    }
                    else
                    {
                        dataProtector = new LocalMachineDpapiDataProtector();
                    }

                    return dataProtector;
                });

            services.AddSingleton<
                IActionResultExecutor<ProfileBlobStreamResult>,
                ProfileBlobStreamResultExecutor>();
            services.AddSingleton<
                IActionResultExecutor<MessageBlobStreamResult>,
                MessageBlobStreamResultExecutor>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IEncryptorFactory, EncryptorFactoryV2>();
            services.AddTransient<BlobWriter>();
            services.AddTransient<BlobReader>();
            services.AddTransient<MalwareScanResultWriter>();
            services.AddSingleton<ProfileKeyService>();
            services.AddSingleton<BlobTokenParser>();

            services.AddGrpc();

            this.AddGrpcClient<AuthorizationClient>(
                services,
                this.BlobsOptions.DomainServicesUrl,
                this.BlobsOptions.DomainServicesUseGrpcWeb);

            this.AddGrpcClient<KeystoreClient>(
                services,
                this.BlobsOptions.KeystoreServicesUrl,
                this.BlobsOptions.KeystoreServicesUseGrpcWeb);

            services
                .AddEDeliveryAuthentication()
                .AddEDeliveryAuthorization();

            this.ConfigureHttpServiceClients(services);

            services.Configure<IISServerOptions>(options =>
            {
                // Remove any restrictions on the maximum request length in ASP.NET Core.
                // The request length in still limited by the maxAllowedContentLength!
                options.MaxRequestBodySize = null;
            });

            services
                .AddControllers()
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.Converters.Add(
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.AllowAnyMethod()
                            .AllowAnyHeader();

                        if (this.BlobsOptions.AllowedCorsOrigins?.Length > 0)
                        {
                            builder.WithOrigins(this.BlobsOptions.AllowedCorsOrigins);
                        }
                        else
                        {
                            builder.AllowAnyOrigin();
                        }
                    });
            });
        }

        public void ConfigureHttpServiceClients(IServiceCollection services)
        {
            void configureMalwareServiceHttpClient(HttpClient client)
            {
                client.BaseAddress =
                    new Uri(
                        this.BlobsOptions.MalwareApiUrl
                        ?? throw new Exception($"Missing setting {nameof(ED.Blobs.BlobsOptions)}.{nameof(ED.Blobs.BlobsOptions.MalwareApiUrl)}"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            var certStore = this.BlobsOptions.MalwareServiceCertificateStore;
            var certStoreLocation = this.BlobsOptions.MalwareServiceCertificateStoreLocation;
            var certThumprint = this.BlobsOptions.MalwareServiceCertificateThumprint;

            X509Certificate2? malwareServiceCert = null;
            if (!string.IsNullOrEmpty(certStore) &&
                certStoreLocation != null &&
                !string.IsNullOrEmpty(certThumprint))
            {
#pragma warning disable CA2000 // Call System.IDisposable.Dispose
                malwareServiceCert = X509Certificate2Utils.LoadX509Certificate(
                    certStore,
                    certStoreLocation.Value,
                    certThumprint);
#pragma warning restore CA2000
            }
            HttpClientHandler configureMalwareServiceMessageHandler()
            {
                var handler = new HttpClientHandler();

                if (malwareServiceCert != null)
                {
                    handler.ClientCertificates.Add(malwareServiceCert);

                    if (this.BlobsOptions.AllowUntrustedCertificates)
                    {
                        handler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    }
                }

                return handler;
            }

            services
                .AddHttpClient(
                    MalwareServiceClient.GeneralPurposeHttpClientName,
                    configureMalwareServiceHttpClient)
                .ConfigurePrimaryHttpMessageHandler(configureMalwareServiceMessageHandler)
                .AddTransientHttpErrorPolicy(
                    p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

            services
                .AddHttpClient(
                    MalwareServiceClient.SubmitHttpClientName,
                    (httpClient) =>
                    {
                        configureMalwareServiceHttpClient(httpClient);
                        httpClient.Timeout = Timeout.InfiniteTimeSpan;
                    })
                .ConfigurePrimaryHttpMessageHandler(configureMalwareServiceMessageHandler);

            services.AddTransient<MalwareServiceClient>();

            services.AddHttpClient<TimestampServiceClient>(
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(
                        this.BlobsOptions.TimestampServiceUrl
                        ?? throw new Exception($"Missing setting {nameof(ED.Blobs.BlobsOptions)}.{nameof(ED.Blobs.BlobsOptions.TimestampServiceUrl)}")
                    );
                })
                .AddTransientHttpErrorPolicy(
                    p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));

            services.AddHttpClient<PdfServicesClient>(
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(
                        this.BlobsOptions.PdfServicesUrl
                        ?? throw new Exception($"Missing setting {nameof(ED.Blobs.BlobsOptions)}.{nameof(ED.Blobs.BlobsOptions.PdfServicesUrl)}")
                    );
                }); // No Polly transient error retiries as the content is streamed
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(exceptionHandlerApp =>
                {
                    exceptionHandlerApp.Run(async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = System.Net.Mime.MediaTypeNames.Text.Plain;
                        await context.Response.WriteAsync("An exception was thrown.");
                    });
                });
            }

            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseCors();

            app.UseEDeliveryAuthentication()
                .UseEDeliveryAuthorization();

            if (this.BlobsOptions.EnableGrpcWeb)
            {
                app.UseGrpcWeb();
            }

            app.UseEndpoints(endpoints =>
            {
                string[] hosts = this.BlobsOptions.GrpcServiceHosts ?? Array.Empty<string>();
                void mapGrpcService<T>() where T : class
                {
                    GrpcServiceEndpointConventionBuilder builder =
                        endpoints.MapGrpcService<T>();

                    if (this.BlobsOptions.EnableGrpcWeb)
                    {
                        builder.EnableGrpcWeb();
                    }

                    builder.RequireHost(hosts).AllowAnonymous();
                }

                mapGrpcService<BlobsService>();

                endpoints.MapControllers();
            });
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
                            RemoteCertificateValidationCallback = delegate { return true; },
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                        }
                    });
            }
        }
    }
}
