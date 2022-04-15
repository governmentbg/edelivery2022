﻿using System.Threading.Tasks;

using ED.DomainServices;

using EDelivery.WebPortal.Grpc;

using Microsoft.Owin.Security.Authorization;

using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class AdministerProfileMessageRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class AdministerProfileMessageRequirementHandler
        : AuthorizationHandler<AdministerProfileMessageRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdministerProfileMessageRequirement requirement)
        {
            if (AuthorizationHelper.AddOrGetCachedRequirementSucceeds(
                context,
                requirement,
                this.RequirementSucceeds))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

        private bool RequirementSucceeds(
            AuthorizationHandlerContext context,
            IAuthorizationRequirement requirement)
        {
            RequirementContext requirementContext =
                AuthorizationHelper.GetRequirementContext(context.Resource);

            int? loginId = (int?)requirementContext.Get(RequirementContextItems.LoginId);
            int? profileId = (int?)requirementContext.Get(RequirementContextItems.ProfileId);

            if (loginId != null &&
                profileId != null &&
                this.HasAdministerProfileAccess(loginId.Value, profileId.Value))
            {
                return true;
            }

            return false;
        }

        private bool HasAdministerProfileAccess(int loginId, int profileId)
        {
            AuthorizationClient authorizationClient = GrpcClientFactory.CreateAuthorizationClient();

            var resp = authorizationClient.HasAdministerProfileAccess(
                new HasAdministerProfileAccessRequest
                {
                    LoginId = loginId,
                    ProfileId = profileId,
                });

            return resp.HasAccess;
        }
    }
}
