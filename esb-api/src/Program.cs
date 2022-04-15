using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using ED.EsbApi;
using Microsoft.Extensions.Configuration;
using Grpc.Net.Client.Web;
using Serilog;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using NJsonSchema;
using NJsonSchema.Generation;
using Mapster;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection.Extensions;

using static ED.DomainServices.Esb.Esb;
using static ED.DomainServices.Authorization;
using FluentValidation.AspNetCore;
using FluentValidation;

[assembly: ApiConventionType(typeof(EsbApiConventions))]

IConfiguration Configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

EsbApiOptions esbApiOptions = new();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Configuration.GetSection("ED:EsbApi").Bind(esbApiOptions);

builder.Services
    .TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services
    .AddControllers()
    .AddNewtonsoftJson()
    .AddFluentValidation();

builder.Services
    .AddValidatorsFromAssemblyContaining<ProfileRegisterDOValidator>();

builder.Services
    .AddApiVersioning(opts =>
    {
        opts.DefaultApiVersion = new ApiVersion(1, 0);
        opts.AssumeDefaultVersionWhenUnspecified = false;
        opts.ReportApiVersions = true;
        opts.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddVersionedApiExplorer(opts =>
    {
        opts.DefaultApiVersion = new ApiVersion(1, 0);
        opts.GroupNameFormat = "'v'VVV";
        opts.SubstituteApiVersionInUrl = true;
    });

// https://blog.rsuter.com/versioned-aspnetcore-apis-with-openapi-generation-and-azure-api-management/
builder.Services
    .AddOpenApiDocument((document, serviceProvider) =>
    {
        document.DocumentName = "v1";
        document.ApiGroupNames = new string[] { "v1" };
        document.Version = "1.0";

        document.SchemaType = SchemaType.OpenApi3;
        document.Title = "ССЕВ";
        document.DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.Null;
    });

builder.Services
    .AddAuthentication(EsbAuthSchemeConstants.EsbAuthScheme)
    .AddEsb();

builder.Services.AddEsbAuthorization();

builder.Services.AddSingleton<IDataProtector>(
    (_) =>
    {
        string? sharedSecretDPKey = esbApiOptions.SharedSecretDPKey;
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

builder.Services.AddSingleton<BlobUrlCreator>();

builder.Services
    .AddOptions<EsbApiOptions>()
    .Bind(builder.Configuration.GetSection("ED:EsbApi"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

AddGrpcClient<EsbClient>(
    builder.Services,
    esbApiOptions.DomainServicesUrl,
    esbApiOptions.DomainServicesUseGrpcWeb);

AddGrpcClient<AuthorizationClient>(
    builder.Services,
    esbApiOptions.DomainServicesUrl,
    esbApiOptions.DomainServicesUseGrpcWeb);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/api/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/api/error-local-development");
}

app.UseOpenApi();
app.UseSwaggerUi3();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app
    .MapGet("/", (HttpContext context) => { context.Response.Redirect("/swagger", true); })
    .ExcludeFromDescription();

app.MapControllers();

TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true;
TypeAdapterConfig.GlobalSettings.Apply(
    new TimestampMapping(),
    new MessageViewDOMapping(),
    new MessageOpenDOMapping());
TypeAdapterConfig.GlobalSettings.Default
    .EnumMappingStrategy(EnumMappingStrategy.ByName);

Log.Logger = new LoggerConfiguration()
    .ReadFrom
    .Configuration(Configuration)
    .CreateLogger();

Log.Information("Serilog started");

app.Run();

void AddGrpcClient<T>(
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
