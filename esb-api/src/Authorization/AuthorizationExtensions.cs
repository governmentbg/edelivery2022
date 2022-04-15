using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ED.EsbApi;

public static class AuthorizationExtensions
{
    public static AuthorizationPolicyBuilder RequireAuthenticatedUserStrict(
        this AuthorizationPolicyBuilder builder)
    {
        builder.AddRequirements(new FailAnonymousAuthorizationRequirement());

        return builder;
    }

    public static IServiceCollection AddEsbAuthorization(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddAuthorization(opts =>
        {
            opts.InvokeHandlersAfterFailure = false;

            opts.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUserStrict()
                .Build();

            opts.AddPolicy(
                Policies.TemplateAccess,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new TemplateAccessRequirement()));

            opts.AddPolicy(
                Policies.ProfilesTargetGroupAccess,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ProfilesTargetGroupAccessRequirement(null)));

            opts.AddPolicy(
                Policies.ProfilesIndividualTargetGroupAccess,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ProfilesTargetGroupAccessRequirement(Constants.IndividualTargetGroupId)));

            opts.AddPolicy(
                Policies.ProfilesOnBehalfOf,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ProfilesOnBehalfOfRequirement()));

            opts.AddPolicy(
                Policies.ReadMessageAsSender,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ReadMessageAsSenderRequirement()));

            opts.AddPolicy(
                Policies.ReadMessageAsRecipient,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ReadMessageAsRecipientRequirement()));

            opts.AddPolicy(
                Policies.ReadInbox,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ReadInboxRequirement()));

            opts.AddPolicy(
                Policies.ReadOutbox,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ReadOutboxRequirement()));

            opts.AddPolicy(
                Policies.SendMessage,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new SendMessageRequirement()));
        });

        services.Scan(scan => scan
            .FromCallingAssembly()
            .AddClasses(c => c.AssignableTo<IAuthorizationHandler>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}
