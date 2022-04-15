
using System;

using log4net;

using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Authorization;
using Microsoft.Owin.Security.Authorization.Infrastructure;

using Owin;

namespace EDelivery.WebPortal.Authorization
{
    public static class AuthorizationExtensions
    {
        public static IAppBuilder UseEDeliveryAuthorization(this IAppBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            app.Use((owinContext, next) =>
            {
                owinContext.Set(new RequirementContext());
                return next();
            });

            app.UseAuthorization(options =>
            {
                options.DefaultPolicy =
                    new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole("User")
                    .Build();

                options.AddPolicy(Policies.ReadMessageAsRecipient,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(new ReadMessageAsRecipientRequirement()));

                options.AddPolicy(Policies.ReadMessageAsSender,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(new ReadMessageAsSenderRequirement()));

                options.AddPolicy(Policies.ReadMessageAsSenderOrRecipient,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(new ReadMessageAsSenderOrRecipientRequirement()));

                options.AddPolicy(Policies.WriteMessage,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(
                                new DenyReadonlyProfileRequirement(),
                                new WriteMessageRequirement()));

                options.AddPolicy(Policies.ForwardMessage,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(
                                new DenyReadonlyProfileRequirement(),
                                new ForwardMessageRequirement()));

                options.AddPolicy(Policies.AdministerProfile,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(
                                new DenyReadonlyProfileRequirement(),
                                new AdministerProfileMessageRequirement()));

                options.AddPolicy(Policies.ListProfileMessage,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(new ListProfileMessageRequirement()));

                options.AddPolicy(Policies.SearchMessageRecipients,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(new DenyReadonlyProfileRequirement()));

                options.AddPolicy(Policies.AdministerProfileRecipientGroups,
                   policyBuilder =>
                       policyBuilder
                           .Combine(options.GetPolicy(Policies.AdministerProfile))
                           .AddRequirements(
                               new AdministerProfileRecipientGroupRequirement()));

                // TODO: rethink with target group matrix
                options.AddPolicy(Policies.SearchMessageRecipientIndividuals,
                   policyBuilder =>
                       policyBuilder
                           .Combine(options.GetPolicy(Policies.SearchMessageRecipients))
                           .AddRequirements(
                               new SearchMessageRecipientIndividualsRequirement()));

                // TODO: rethink with target group matrix
                options.AddPolicy(Policies.SearchMessageRecipientLegalEntities,
                   policyBuilder =>
                       policyBuilder
                           .Combine(options.GetPolicy(Policies.SearchMessageRecipients))
                           .AddRequirements(
                               new SearchMessageRecipientLegalEntitiesRequirement()));

                options.AddPolicy(Policies.WriteCodeMessage,
                    policyBuilder =>
                        policyBuilder
                            .Combine(options.DefaultPolicy)
                            .AddRequirements(
                                new DenyReadonlyProfileRequirement(),
                                new WriteCodeMessageRequirement()));

                IAuthorizationHandler[] handlers = new IAuthorizationHandler[]
                {
                    new AdministerProfileMessageRequirementHandler(),
                    new DenyReadonlyProfileRequirementHandler(),
                    new ReadMessageAsRecipientRequirementHandler(),
                    new ReadMessageAsSenderRequirementHandler(),
                    new ReadMessageAsSenderOrRecipientRequirementHandler(),
                    new WriteMessageRequirementHandler(),
                    new ForwardMessageRequirementHandler(),
                    new ListProfileMessageRequirementHandler(),
                    new AdministerProfileRecipientGorupRequirementHandler(),
                    new SearchMessageRecipientIndividualsRequirementHandler(),
                    new SearchMessageRecipientLegalEntitiesRequirementHandler(),
                    new WriteCodeMessageRequirementHandler(),
                };

                var policyProvider = new DefaultAuthorizationPolicyProvider(options);
                var service = new DefaultAuthorizationService(
                    policyProvider,
                    handlers,
                    new Log4NetLoggerWrapper(LogManager.GetLogger("Global").Logger),
                    new DefaultAuthorizationHandlerContextFactory(),
                    new DefaultAuthorizationEvaluator());

                options.Dependencies.PolicyProvider = policyProvider;
                options.Dependencies.Service = service;
            });

            return app;
        }
    }
}
