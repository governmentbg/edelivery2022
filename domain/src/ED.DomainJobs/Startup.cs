using System;
using Autofac;
using ED.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Serilog;

namespace ED.DomainJobs
{
    public class Startup
    {
        public IConfiguration Configuration { get; init; }

        public IWebHostEnvironment Environment { get; init; }

        public EDModule[] Modules { get; init; }

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;

            this.Modules = new EDModule[]
            {
                new DomainModule(configuration, environment),
            };
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterModules(this.Modules);

            services.AddOptions<DomainJobsOptions>()
                .Bind(this.Configuration.GetSection("ED:DomainJobs"))
                .ValidateDataAnnotations();

            services.AddOptions<ClientsOptions>()
                .Bind(this.Configuration.GetSection("ED:Clients"))
                .ValidateDataAnnotations();

            services.AddHttpClient<LinkMobilityServiceClient>(
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(
                        this.Configuration.GetValue<string>("ED:Clients:LinkMobility:ApiUrl")
                        ?? throw new Exception($"Missing setting {nameof(LinkMobilityOptions)}.{nameof(LinkMobilityOptions.ApiUrl)}")
                    );
                })
                .AddTransientHttpErrorPolicy(
                    p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300)));

            services.AddHttpClient<PushNotificationServiceClient>();

            if (this.Configuration.GetValue<int>("ED:DomainJobs:EmailJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<EmailJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:SmsJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<SmsJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:PushNotificationJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<PushNotificationJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:ViberJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<ViberJob>();
            }
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("OK");
                });
            });
        }
    }
}
