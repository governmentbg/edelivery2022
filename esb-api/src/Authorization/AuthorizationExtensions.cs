using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ED.EsbApi;

public static class AuthorizationExtensions
{
    public static AuthorizationPolicyBuilder RequireAuthenticatedUserStrict(
        this AuthorizationPolicyBuilder builder)
    {
        builder.AddRequirements(new StrictDenyAnonymousAuthorizationRequirement());

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
                Policies.OboTemplateAccess,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new OboTemplateAccessRequirement()));

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
                Policies.OboProfilesAccess,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new OboProfilesAccessRequirement()));

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
                Policies.ReadForwardedMessageAsRecipient,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new ReadForwardedMessageAsRecipientRequirement()));

            opts.AddPolicy(
                Policies.OboReadInbox,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new OboProfilesAccessRequirement()));

            opts.AddPolicy(
                Policies.OboReadOutbox,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new OboProfilesAccessRequirement()));

            opts.AddPolicy(
                Policies.SendMessage,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new SendMessageRequirement()));

            opts.AddPolicy(
                Policies.OboSendMessage,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new OboSendMessageRequirement()));

            opts.AddPolicy(
                Policies.OboStorageAccess,
                policyBuilder => policyBuilder
                    .Combine(opts.DefaultPolicy)
                    .AddRequirements(
                        new OboProfilesAccessRequirement()));
        });

        services.Scan(scan => scan
            .FromCallingAssembly()
            .AddClasses(c => c.AssignableTo<IAuthorizationHandler>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }
}
