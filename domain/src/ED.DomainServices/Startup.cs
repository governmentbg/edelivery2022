using System;
using Autofac;
using ED.Domain;
using Mapster;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ED.DomainServices
{
    public class Startup
    {
        public IConfiguration Configuration { get; init; }

        public IWebHostEnvironment Environment { get; init; }

        public EDModule[] Modules { get; init; }

        private DomainServicesOptions DomainServicesOptions { get; init; }

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
            this.DomainServicesOptions = new DomainServicesOptions();
            configuration.GetSection("ED:DomainServices").Bind(this.DomainServicesOptions);

            this.Modules = new EDModule[]
            {
                new DomainModule(configuration, environment),
            };

            TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true;
            TypeAdapterConfig.GlobalSettings.Apply(
                new TimestampMapping(),
                new GuidMapping(),
                new ByteStringMapping(),
                new RepeatedFieldMapping(),
                new MapFieldMapping(),
                new OneOfFieldMapping());
            TypeAdapterConfig.GlobalSettings.Default
                .EnumMappingStrategy(EnumMappingStrategy.ByName);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterModules(this.Modules);

            services.Configure<DomainServicesOptions>(
                this.Configuration.GetSection("ED:DomainServices"));

            services.AddGrpc(options =>
            {
                if (!this.Environment.IsProduction())
                {
                    var delay = this.DomainServicesOptions.DelayGrpcMethodsForTesting;
                    if (delay != null &&
                        delay > TimeSpan.Zero)
                    {
                        options.Interceptors.Add<DelayInterceptor>(delay);
                    }
                }

                var maxReceiveMessageSize = this.DomainServicesOptions.MaxReceiveMessageSize;
                if (maxReceiveMessageSize != null)
                {
                    options.MaxReceiveMessageSize = maxReceiveMessageSize * 1024 * 1024;
                }
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            foreach (var module in this.Modules)
            {
                builder.RegisterModule(module);
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

            if (this.DomainServicesOptions.EnableGrpcWeb)
            {
                app.UseGrpcWeb();
            }

            app.UseEndpoints(endpoints =>
            {
                string[] hosts = this.DomainServicesOptions.GrpcServiceHosts ?? Array.Empty<string>();
                void mapGrpcService<T>() where T : class
                {
                    GrpcServiceEndpointConventionBuilder builder =
                        endpoints.MapGrpcService<T>();

                    if (this.DomainServicesOptions.EnableGrpcWeb)
                    {
                        builder.EnableGrpcWeb();
                    }

                    builder.RequireHost(hosts);
                }

                mapGrpcService<DomainService>();
                mapGrpcService<TemplateService>();
                mapGrpcService<NomenclatureService>();
                mapGrpcService<MessageService>();
                mapGrpcService<ProfileService>();
                mapGrpcService<AuthorizationService>();
                mapGrpcService<AdminService>();
                mapGrpcService<CodeMessageService>();
                mapGrpcService<EsbService>();
                mapGrpcService<IntegrationServiceService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
