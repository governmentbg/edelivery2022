using System;
using System.Net.Http;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ED.Keystore
{
    public class Startup
    {
        private KeystoreOptions KeystoreOptions { get; init; }
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.KeystoreOptions = new KeystoreOptions();
            configuration.GetSection("ED:Keystore").Bind(this.KeystoreOptions);
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KeystoreOptions>(
                this.Configuration.GetSection("ED:Keystore"));

            services.AddGrpc();

            services.AddSingleton<MsKspCngKeystore>();
            services.AddTransient<CngKeystoreResolver>(
                serviceProvider => cngProvider => cngProvider switch
                {
                    "Microsoft Software Key Storage Provider" => serviceProvider.GetRequiredService<MsKspCngKeystore>(),
                    _ => throw new Exception("Unsupported CNG provider"),
                });

            // add a dummy named GrpcClient so that the GrpcClientFactory
            // is added to the registered services and can always be resolved
            // even if we wont be using it to create any clients
            services.AddGrpcClient<KeySync.KeySyncClient>("dummy");

            foreach (var (deploymentName, deploymentUrl, deploymentUseGrpcWeb)
                in this.KeystoreOptions.GetOtherDeployments())
            {

                this.AddNamedGrpcClient<KeySync.KeySyncClient>(
                    services,
                    deploymentName,
                    deploymentUrl,
                    deploymentUseGrpcWeb);
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSerilogRequestLogging();

            app.UseRouting();

            if (this.KeystoreOptions.EnableGrpcWeb)
            {
                app.UseGrpcWeb();
            }

            app.UseEndpoints(endpoints =>
            {
                string[] hosts = this.KeystoreOptions.GrpcServiceHosts ?? Array.Empty<string>();
                void mapGrpcService<T>() where T : class
                {
                    GrpcServiceEndpointConventionBuilder builder =
                        endpoints.MapGrpcService<T>();

                    if (this.KeystoreOptions.EnableGrpcWeb)
                    {
                        builder.EnableGrpcWeb();
                    }

                    builder.RequireHost(hosts);
                }

                mapGrpcService<KeySyncService>();
                mapGrpcService<KeystoreService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        private void AddNamedGrpcClient<T>(
            IServiceCollection services,
            string name,
            string? address,
            bool useGrpcWeb)
            where T : class
        {
            address = address ?? throw new Exception($"Missing address for GrpcClient {typeof(T).Name}");

            IHttpClientBuilder grpcHttpClientBuilder = services.AddGrpcClient<T>(
                name,
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
