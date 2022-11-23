using ED.DomainServices;
using EDelivery.WebPortal.Grpc;
using Microsoft.Owin.Security.Authorization;
using System.Threading.Tasks;
using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class MessageAccessKeyRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class MessageAccessKeyRequirementHandler
        : AuthorizationHandler<MessageAccessKeyRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            MessageAccessKeyRequirement requirement)
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
            int? messageId = (int?)requirementContext.Get(RequirementContextItems.MessageId);

            if (profileId != null &&
                messageId != null &&
                this.HasMessageAccessKey(
                    profileId.Value,
                    messageId.Value))
            {
                return true;
            }

            return false;
        }

        private bool HasMessageAccessKey(int profileId, int messageId)
        {
            AuthorizationClient authorizationClient = GrpcClientFactory.CreateAuthorizationClient();

            var resp = authorizationClient.HasMessageAccessKey(
                new HasMessageAccessKeyRequest
                {
                    ProfileId = profileId,
                    MessageId = messageId,
                });

            return resp.HasAccess;
        }
    }
}
