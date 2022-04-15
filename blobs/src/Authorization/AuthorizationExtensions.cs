using System;
using System.Collections.Generic;
using System.Security.Claims;
using Invio.Extensions.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace ED.Blobs
{
    public static class AuthorizationExtensions
    {
        private const string WebPortalAuthScheme = "EDelivery.WebPortal";
        private const string AdminPanelAuthScheme = "ED.AdminPanel";

        public static IServiceCollection AddEDeliveryAuthentication(
            this IServiceCollection services)
        {
            services.AddOptions<JwtBearerOptions>(WebPortalAuthScheme)
                .Configure<IDataProtector>(
                    (options, dataProtector) => {
                        options.SecurityTokenValidators.Add(
                            new LegacyOAuthSecurityTokenHandler(
                                dataProtector.CreateProtector(WebPortalAuthScheme),
                                // add the authentication scheme as a claim
                                // so that we can check it in the requirements
                                new Claim(ClaimTypes.AuthenticationMethod, WebPortalAuthScheme)));
                    });

            services.AddOptions<JwtBearerOptions>(AdminPanelAuthScheme)
                .Configure<IDataProtector>(
                    (options, dataProtector) => {
                        options.SecurityTokenValidators.Add(
                            new LegacyOAuthSecurityTokenHandler(
                                dataProtector.CreateProtector(AdminPanelAuthScheme),
                                // add the authentication scheme as a claim
                                // so that we can check it in the requirements
                                new Claim(ClaimTypes.AuthenticationMethod, AdminPanelAuthScheme)));
                    });

            services.AddAuthentication()
                .AddJwtBearer(WebPortalAuthScheme, (_) => { })
                .AddJwtBearer(AdminPanelAuthScheme, (_) => { })
                .AddJwtBearerQueryStringAuthentication(options =>
                {
                    options.QueryStringParameterName = "t";
                    options.QueryStringBehavior = QueryStringBehaviors.None;
                });

            return services;
        }
        
        public static IServiceCollection AddEDeliveryAuthorization(this IServiceCollection services)
        {
            services
                .AddAuthorization(auth =>
                {
                    auth.AddPolicy(Policies.WriteProfileBlob,
                        policyBuilder =>
                            policyBuilder
                                .AddAuthenticationSchemes(WebPortalAuthScheme)
                                .RequireAuthenticatedUser()
                                .RequireRole("User")
                                .AddRequirements(
                                    new DenyReadonlyProfileRequirement(),
                                    new ProfileAccessRequirement()));

                    auth.AddPolicy(Policies.WriteSystemRegistrationBlob,
                        policyBuilder =>
                            policyBuilder
                                .AddAuthenticationSchemes(
                                    WebPortalAuthScheme,
                                    AdminPanelAuthScheme)
                                .RequireAuthenticatedUser()
                                .AddRequirements(
                                    new RolesPerAuthSchemeRequirement(
                                        new Dictionary<string, IEnumerable<string>>
                                        {
                                            { WebPortalAuthScheme, new [] { "User" } },
                                            { AdminPanelAuthScheme, Array.Empty<string>() }
                                        })));

                    auth.AddPolicy(Policies.WriteSystemTemplateBlob,
                        policyBuilder =>
                            policyBuilder
                                .AddAuthenticationSchemes(AdminPanelAuthScheme)
                                .RequireAuthenticatedUser());
                })
                .AddSingleton<IAuthorizationHandler, ProfileAccessRequirementHandler>()
                .AddSingleton<IAuthorizationHandler, DenyReadonlyProfileRequirementHandler>()
                .AddSingleton<IAuthorizationHandler, RolesPerAuthSchemeRequirementHanlder>();

            return services;
        }

        public static IApplicationBuilder UseEDeliveryAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();

            // This should come after authentication but before any logging middleware.
            app.UseJwtBearerQueryString();

            return app;
        }

        public static IApplicationBuilder UseEDeliveryAuthorization(this IApplicationBuilder app)
        {
            app.Use(next => httpContext =>
            {
                RequirementContext requirementContext = new();

                requirementContext.Set(
                    RequirementContextItems.LoginId,
                    () => httpContext.GetLoginId());

                requirementContext.Set(
                    RequirementContextItems.ProfileId,
                    () =>
                    {
                        if (int.TryParse(httpContext.GetFromRouteOrQuery("profileId"), out int profileId))
                        {
                            return profileId;
                        }
                        return null;
                    });

                requirementContext.Set(
                    RequirementContextItems.MessageId,
                    () =>
                    {
                        if (int.TryParse(httpContext.GetFromRouteOrQuery("messageId"), out int messageId))
                        {
                            return messageId;
                        }
                        return null;
                    });

                httpContext.Items.Add(nameof(RequirementContext), requirementContext);

                return next(httpContext);
            });

            app.UseAuthorization();

            return app;
        }
    }
}
