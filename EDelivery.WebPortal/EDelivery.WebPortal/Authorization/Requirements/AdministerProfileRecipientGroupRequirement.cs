using System.Threading.Tasks;

using ED.DomainServices;

using EDelivery.WebPortal.Grpc;

using Microsoft.Owin.Security.Authorization;

using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class AdministerProfileRecipientGroupRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class AdministerProfileRecipientGorupRequirementHandler
        : AuthorizationHandler<AdministerProfileRecipientGroupRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdministerProfileRecipientGroupRequirement requirement)
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

            int? profileId = (int?)requirementContext.Get(RequirementContextItems.ProfileId);
            int? recipientGroupId = (int?)requirementContext.Get(RequirementContextItems.RecipientGroupId);

            if (profileId != null &&
                recipientGroupId != null &&
                this.HasAdministerProfileRecipientGroupAccess(profileId.Value, recipientGroupId.Value))
            {
                return true;
            }

            return false;
        }

        private bool HasAdministerProfileRecipientGroupAccess(
            int profileId,
            int recipientGroupId)
        {
            AuthorizationClient authorizationClient = GrpcClientFactory.CreateAuthorizationClient();

            var resp = authorizationClient.HasAdministerProfileRecipientGroupAccess(
                new HasAdministerProfileRecipientGroupAccessRequest
                {
                    ProfileId = profileId,
                    RecipientGroupId = recipientGroupId,
                });

            return resp.HasAccess;
        }
    }
}
