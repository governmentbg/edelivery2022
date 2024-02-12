using ED.DomainServices;

using EDelivery.WebPortal.Grpc;

using Microsoft.Owin.Security.Authorization;

using System.Threading.Tasks;

using static ED.DomainServices.Authorization;

namespace EDelivery.WebPortal.Authorization
{
    public class ReplyMessageRequirement : IAuthorizationRequirement
    {
    }

    // NOTE! ResourceAuthorizeAttribute's call to IAuthorizationService.AuthorizeAsync is blocking (using .Result)
    // see https://github.com/DavidParks8/Owin-Authorization/issues/57
    // Because of that all async should actually run synchronously or else a deadlock will occur
    public class ReplyMessageRequirementHandler
        : AuthorizationHandler<ReplyMessageRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ReplyMessageRequirement requirement)
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
            int? templateId = (int?)requirementContext.Get(RequirementContextItems.TemplateId);
            int? forwardingMessageId = (int?)requirementContext.Get(RequirementContextItems.ForwardingMessageId);

            if (loginId == null
                || profileId == null
                || messageId == null
                || templateId == null)
            {
                return false;
            }

            bool hasWriteMessageAccess = this.HasWriteMessageAccess(
                loginId.Value,
                profileId.Value,
                templateId.Value);

            bool hasReadMessageAccess = this.HasReadMessageAsRecipientAccess(
                loginId.Value,
                profileId.Value,
                messageId.Value)
                || this.HasReadMessageThroughForwardingAsRecipientAccess(
                    loginId.Value,
                    profileId.Value,
                    messageId.Value,
                    forwardingMessageId);

            return hasWriteMessageAccess && hasReadMessageAccess;
        }

        private bool HasWriteMessageAccess(
            int loginId,
            int profileId,
            int templateId)
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

        private bool HasReadMessageAsRecipientAccess(
            int loginId,
            int profileId,
            int messageId)
        {
            AuthorizationClient authorizationClient =
                GrpcClientFactory.CreateAuthorizationClient();

            HasAccessResponse resp =
                authorizationClient.HasReadMessageAsRecipientAccess(
                    new HasReadMessageAsRecipientAccessRequest
                    {
                        LoginId = loginId,
                        ProfileId = profileId,
                        MessageId = messageId,
                    });

            return resp.HasAccess;
        }

        private bool HasReadMessageThroughForwardingAsRecipientAccess(
            int loginId,
            int profileId,
            int messageId,
            int? forwardingMessageId)
        {
            AuthorizationClient authorizationClient =
                GrpcClientFactory.CreateAuthorizationClient();

            HasAccessResponse resp =
                authorizationClient.HasReadMessageThroughForwardingAsRecipientAccess(
                    new HasReadMessageThroughForwardingAsRecipientAccessRequest
                    {
                        LoginId = loginId,
                        ProfileId = profileId,
                        MessageId = messageId,
                        ForwardingMessageId = forwardingMessageId,
                    });

            return resp.HasAccess;
        }
    }
}
