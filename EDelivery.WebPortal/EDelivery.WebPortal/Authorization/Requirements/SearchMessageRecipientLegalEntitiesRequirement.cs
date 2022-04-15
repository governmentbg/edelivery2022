using System.Threading.Tasks;

using ED.DomainServices;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Grpc;

using Microsoft.Owin.Security.Authorization;

using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class SearchMessageRecipientLegalEntitiesRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class SearchMessageRecipientLegalEntitiesRequirementHandler
        : AuthorizationHandler<SearchMessageRecipientLegalEntitiesRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SearchMessageRecipientLegalEntitiesRequirement requirement)
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

            if (profileId != null &&
                this.HasAccessTargetGroupSearch(profileId.Value))
            {
                return true;
            }

            return false;
        }

        private bool HasAccessTargetGroupSearch(int profileId)
        {
            AuthorizationClient authorizationClient = GrpcClientFactory.CreateAuthorizationClient();

            var resp = authorizationClient.HasAccessTargetGroupSearch(
                new HasAccessTargetGroupSearchRequest
                {
                    ProfileId = profileId,
                    TargetGroupId = (int)TargetGroupId.LegalEntity
                });

            return resp.HasAccess;
        }
    }
}
