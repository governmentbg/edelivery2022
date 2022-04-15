using ED.DomainServices;
using EDelivery.WebPortal.Grpc;
using Microsoft.Owin.Security.Authorization;
using System.Threading.Tasks;
using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class ReadMessageAsSenderOrRecipientRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class ReadMessageAsSenderOrRecipientRequirementHandler
        : AuthorizationHandler<ReadMessageAsSenderOrRecipientRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ReadMessageAsSenderOrRecipientRequirement requirement)
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
            int? messageId = (int?)requirementContext.Get(RequirementContextItems.MessageId);

            if (loginId != null &&
                profileId != null &&
                messageId != null &&
                this.HasReadMessageAsSenderOrRecipientAccess(
                    loginId.Value,
                    profileId.Value,
                    messageId.Value))
            {
                return true;
            }

            return false;
        }

        private bool HasReadMessageAsSenderOrRecipientAccess(int loginId, int profileId, int messageId)
        {
            AuthorizationClient authorizationClient = GrpcClientFactory.CreateAuthorizationClient();

            var resp = authorizationClient.HasReadMessageAsSenderOrRecipientAccess(
                new HasReadMessageAsSenderOrRecipientAccessRequest
                {
                    LoginId = loginId,
                    ProfileId = profileId,
                    MessageId = messageId,
                });

            return resp.HasAccess;
        }
    }
}
