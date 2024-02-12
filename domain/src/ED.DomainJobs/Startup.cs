using System;
using System.Net.Http.Headers;
using System.Text;
using Autofac;
using ED.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
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

            services.AddMemoryCache();
            services.AddFusionCache();

            services.AddSingleton<IDataProtector>(
                (_) =>
                {
                    string? sharedSecretDPKey =
                        this.Configuration.GetValue<string?>("ED:Authentication:SharedSecretDPKey");
                    IDataProtector dataProtector;
                    if (!string.IsNullOrEmpty(sharedSecretDPKey))
                    {
                        dataProtector = new SharedSecretDataProtector(sharedSecretDPKey);
                    }
                    else
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        dataProtector = new LocalMachineDpapiDataProtector();
#pragma warning restore CA1416 // Validate platform compatibility
                    }

                    return dataProtector;
                });

            services.AddSingleton<BlobUrlCreator>();

            services.AddOptions<DomainJobsOptions>()
                .Bind(this.Configuration.GetSection("ED:DomainJobs"))
                .ValidateDataAnnotations();

            services.AddOptions<ClientsOptions>()
                .Bind(this.Configuration.GetSection("ED:Clients"))
                .ValidateDataAnnotations();

            services.AddOptions<ETranslationOptions>()
               .Bind(this.Configuration.GetSection("ED:Clients:ETranslation"))
               .ValidateDataAnnotations();

            services.AddOptions<AuthenticationOptions>()
                .Bind(this.Configuration.GetSection("ED:Authentication"))
                .ValidateDataAnnotations();

            services.AddOptions<InfosystemsOptions>()
               .Bind(this.Configuration.GetSection("ED:Clients:Infosystems"))
               .ValidateDataAnnotations();

            services.AddOptions<DataPortalOptions>()
               .Bind(this.Configuration.GetSection("ED:Clients:DataPortal"))
               .ValidateDataAnnotations();

            services.AddHttpClient<InfosystemsServiceClient>(
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(
                        this.Configuration.GetValue<string>("ED:Clients:Infosystems:ApiUrl")
                        ?? throw new Exception($"Missing setting {nameof(InfosystemsOptions)}.{nameof(InfosystemsOptions.ApiUrl)}")
                    );

                    byte[] textAsBytes = Encoding.UTF8.GetBytes(
                        $"{this.Configuration.GetValue<string>("ED:Clients:Infosystems:ApiUserName")}:" +
                        $"{this.Configuration.GetValue<string>("ED:Clients:Infosystems:ApiPassword")}");

                    httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(textAsBytes));
                })
                .AddTransientHttpErrorPolicy(
                    p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300)));

            services.AddHttpClient<PushNotificationServiceClient>();

            services.AddHttpClient<ETranslationClient>(
                httpClient =>
                {
                    httpClient.BaseAddress = new Uri(
                        this.Configuration.GetValue<string>("ED:Clients:ETranslation:ApiUrl")
                        ?? throw new Exception($"Missing setting {nameof(ETranslationOptions)}.{nameof(ETranslationOptions.ApiUrl)}")
                    );
                })
                .AddTransientHttpErrorPolicy(
                    p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300)));

            services.AddHttpClient<DataPortalServiceClient>(
               httpClient =>
               {
                   httpClient.BaseAddress = new Uri(
                       this.Configuration.GetValue<string>("ED:Clients:DataPortal:ApiUrl")
                       ?? throw new Exception($"Missing setting {nameof(DataPortalOptions)}.{nameof(DataPortalOptions.ApiUrl)}")
                   );
               })
               .AddTransientHttpErrorPolicy(
                   p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(300)));

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
            if (this.Configuration.GetValue<int>("ED:DomainJobs:SmsDeliveryCheckJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<SmsDeliveryCheckJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:ViberDeliveryCheckJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<ViberDeliveryCheckJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:TranslationJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<TranslationJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:TranslationClosureJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<TranslationClosureJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:DeliveredTicketsJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<DeliveredTicketsJob>();
            }
            if (this.Configuration.GetValue<int>("ED:DomainJobs:DataPortalJob:PeriodInSeconds") != 0)
            {
                services.AddHostedService<DataPortalJob>();
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
