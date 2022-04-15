using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Authorization;

namespace ED.EsbApi;

public class ReadMessageAsSenderRequirement : IAuthorizationRequirement
{
}

public class ReadMessageAsSenderRequirementHandler : AuthorizationHandler<ReadMessageAsSenderRequirement>
{
    private readonly HttpContext httpContext;
    private readonly AuthorizationClient authorizationClient;

    public ReadMessageAsSenderRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        AuthorizationClient authorizationClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(ReadMessageAsSenderRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.authorizationClient = authorizationClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadMessageAsSenderRequirement requirement)
    {
        int profileId = context.User.GetAuthenticatedUserProfileId();
        int loginId = context.User.GetAuthenticatedUserLoginId();
        string? messageIdParam = this.httpContext.GetFromRouteOrQuery("messageId");
        bool isRequirementMet = false;

        if (int.TryParse(messageIdParam, out int messageId))
        {
            DomainServices.HasAccessResponse resp =
                await this.authorizationClient.HasReadMessageAsSenderAccessAsync(
                    new DomainServices.HasReadMessageAsSenderAccessRequest
                    {
                        MessageId = messageId,
                        ProfileId = profileId,
                        LoginId = loginId,
                    });

            if (resp.HasAccess)
            {
                isRequirementMet = true;
            }
        }

        if (isRequirementMet)
        {
            context.Succeed(requirement);
        }
        else
        {
            this.httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }

        return;
    }
}
