using System;
using System.Net.Http;
using Blazored.Modal;
using ED.AdminPanel.Blazor;
using ED.AdminPanel.Resources;
using Grpc.Net.Client.Web;
using Grpc.Net.ClientFactory;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using static ED.DomainServices.Admin.Admin;
using static ED.DomainServices.Nomenclatures.Nomenclature;
using static ED.DomainServices.Templates.Template;

#nullable enable

namespace ED.AdminPanel
{
    public class Startup
    {
        public const string GrpcReportsClient = "ReportsClient";

        private AdminPanelOptions Options { get; init; }
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            this.Configuration = configuration;
            this.Environment = environment;
            this.Options = new AdminPanelOptions();
            configuration.GetSection("ED:AdminPanel")
                .Bind(this.Options);
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // options
            services.AddOptions<AdminPanelOptions>()
                .Bind(this.Configuration.GetSection("ED:AdminPanel"))
                .ValidateDataAnnotations();
            // TODO: use eager options validation in .NET 6
            // see https://github.com/dotnet/runtime/issues/36391
            // .ValidateOnStart();

            // identity
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlServer(
                    this.Configuration.GetConnectionString("DefaultConnection")));
            services
                .AddDefaultIdentity<IdentityUser<int>>(
                    options =>
                    {
                        options.SignIn.RequireConfirmedAccount = true;
                    })
                .AddEntityFrameworkStores<IdentityDbContext>();

            services.AddSingleton<IDataProtector>(
                (_) =>
                {
                    string? sharedSecretDPKey = this.Options.SharedSecretDPKey;
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

            // used as a substitute for ConfigureApplicationCookie
            // with the added ability to inject dependencies (IDataProtector)
            services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
                .Configure<IDataProtector>(
                    (options, dataProtector) =>
                    {
                        options.TicketDataFormat =
                            new TicketDataFormat(
                                dataProtector.CreateProtector("ED.AdminPanel"));
                        options.Cookie.HttpOnly = false;
                    });
            services.AddAuthorization(
                auth =>
                {
                    auth.DefaultPolicy =
                        auth.FallbackPolicy =
                            new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                });

            // mvc and razor pages
            services.AddLocalization();
            services.AddControllers(options => options.SuppressAsyncSuffixInActionNames = true);
            services.AddRazorPages()
                .AddViewLocalization();
            services.AddDatabaseDeveloperPageExceptionFilter();

            // blazor
            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser<int>>>();
            services.AddScoped<AuthenticationStateHelper>();
            services.AddSingleton<CommonStringLocalizer>();
            services.AddBlazoredModal();

            // grpc
            this.AddNamedGrpClient<AdminClient>(
                Startup.GrpcReportsClient,
                services,
                this.Options.DomainServicesUrl,
                this.Options.DomainServicesUseGrpcWeb);
            this.AddGrpcClient<AdminClient>(
                services,
                this.Options.DomainServicesUrl,
                this.Options.DomainServicesUseGrpcWeb);
            this.AddGrpcClient<TemplateClient>(
                services,
                this.Options.DomainServicesUrl,
                this.Options.DomainServicesUseGrpcWeb);
            this.AddGrpcClient<NomenclatureClient>(
                services,
                this.Options.DomainServicesUrl,
                this.Options.DomainServicesUseGrpcWeb);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseBlockedIdentityPages();

            app.UseRouting();

            var supportedCultures = new[] { "bg", "en" };
            app.UseRequestLocalization(
                new RequestLocalizationOptions()
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapBlazorHub()
                    // Allowing anonymous acces to the blazor hub
                    // as we use the CultureSelector component on
                    // non-blazor pages as well.
                    // Consider creating a _CultureSelectorPartial with the
                    // same functionality and disable anonymous access.
                    .AllowAnonymous();
                endpoints.MapFallbackToPage("/_Host");
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
                            RemoteCertificateValidationCallback = delegate
                            { return true; },
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
                        }
                    });
            }
        }

        private void AddNamedGrpClient<T>(
            string name,
            IServiceCollection services,
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
                    options.ChannelOptionsActions.Add(
                        (opts) =>
                        {
                            opts.MaxReceiveMessageSize = null;
                        });
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
