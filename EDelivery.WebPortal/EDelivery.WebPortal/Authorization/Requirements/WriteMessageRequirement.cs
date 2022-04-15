using ED.DomainServices;
using EDelivery.WebPortal.Grpc;
using Microsoft.Owin.Security.Authorization;
using System.Threading.Tasks;
using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class WriteMessageRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class WriteMessageRequirementHandler
        : AuthorizationHandler<WriteMessageRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            WriteMessageRequirement requirement)
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
            int? templateId = (int?)requirementContext.Get(RequirementContextItems.TemplateId);

            if (loginId != null &&
                profileId != null &&
                templateId != null &&
                this.HasWriteMessageAccess(
                    loginId.Value,
                    profileId.Value,
                    templateId.Value))
            {
                return true;
            }

            return false;
        }

        private bool HasWriteMessageAccess(int loginId, int profileId, int templateId)
        {
            AuthorizationClient authorizationClient = GrpcClientFactory.CreateAuthorizationClient();

            var resp = authorizationClient.HasWriteMessageAccess(
                new HasWriteMessageAccessRequest
                {
                    LoginId = loginId,
                    ProfileId = profileId,
                    TemplateId = templateId,
                });

            return resp.HasAccess;
        }
    }
}
