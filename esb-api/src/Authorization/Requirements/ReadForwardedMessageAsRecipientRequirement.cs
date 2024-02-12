using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Authorization;

namespace ED.EsbApi;

public class ReadForwardedMessageAsRecipientRequirement : IAuthorizationRequirement
{
}

//public class ReadForwardedMessageAsRecipientRequirementHandler : AuthorizationHandler<ReadForwardedMessageAsRecipientRequirement>
//{
//    private readonly HttpContext httpContext;
//    private readonly AuthorizationClient authorizationClient;

//    public ReadForwardedMessageAsRecipientRequirementHandler(
//        IHttpContextAccessor httpContextAccessor,
//        AuthorizationClient authorizationClient)
//    {
//        if (httpContextAccessor.HttpContext == null)
//        {
//            throw new Exception($"'{nameof(ReadMessageAsRecipientRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
//        }

//        this.httpContext = httpContextAccessor.HttpContext;
//        this.authorizationClient = authorizationClient;
//    }

//    protected override async Task HandleRequirementAsync(
//        AuthorizationHandlerContext context,
//        ReadForwardedMessageAsRecipientRequirement requirement)
//    {
//        int profileId = context.User.GetAuthenticatedUserProfileId();
//        int loginId = context.User.GetAuthenticatedUserLoginId();
//        string? messageIdParam = this.httpContext.GetFromRouteOrQuery("messageId");

//        if (int.TryParse(messageIdParam, out int messageId))
//        {
//            DomainServices.HasAccessResponse resp =
//                await this.authorizationClient.HasReadMessageAsRecipientAccessAsync(
//                    new DomainServices.HasReadMessageAsRecipientAccessRequest
//                    {
//                        MessageId = messageId,
//                        ProfileId = profileId,
//                        LoginId = loginId,
//                    });

//            if (resp.HasAccess)
//            {
//                context.Succeed(requirement);
//                return;
//            }
//        }

//        context.Fail();
//        return;
//    }
//}

public class ReadForwardedMessageAsRecipientRequirementHandler : AuthorizationHandler<ReadForwardedMessageAsRecipientRequirement>
{
    private readonly HttpContext httpContext;
    private readonly AuthorizationClient authorizationClient;

    public ReadForwardedMessageAsRecipientRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        AuthorizationClient authorizationClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(ReadForwardedMessageAsRecipientRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.authorizationClient = authorizationClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadForwardedMessageAsRecipientRequirement requirement)
    {
        int? profileId = context.User.GetAuthenticatedUserProfileIdOrDefault();
        if (!profileId.HasValue)
        {
            context.Fail();
            return;
        }

        int? loginId = context.User.GetAuthenticatedUserLoginIdOrDefault();
        if (!loginId.HasValue)
        {
            context.Fail();
            return;
        }

        string? messageIdParam = this.httpContext.GetFromRouteOrQuery("forwardedMessageId");

        if (int.TryParse(messageIdParam, out int messageId))
        {
            DomainServices.HasAccessResponse resp =
                await this.authorizationClient.HasForwardMessageAccessAsync(
                    new DomainServices.HasForwardMessageAccessRequest
                    {
                        MessageId = messageId,
                        ProfileId = profileId.Value,
                        LoginId = loginId.Value,
                    });

            if (resp.HasAccess)
            {
                context.Succeed(requirement);
                return;
            }
        }

        context.Fail();
        return;
    }
}
