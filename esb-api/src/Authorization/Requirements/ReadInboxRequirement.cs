using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

public class ReadInboxRequirement : IAuthorizationRequirement
{
}

public class ReadInboxRequirementHandler : AuthorizationHandler<ReadInboxRequirement>
{
    private readonly HttpContext httpContext;
    private readonly EsbClient esbClient;

    public ReadInboxRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        EsbClient esbClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(ReadMessageAsSenderRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.esbClient = esbClient;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReadInboxRequirement requirement)
    {
        int? representedProfileId = context.User.GetAuthenticatedUserRepresentedProfileId();
        bool isRequirementMet = false;

        if (representedProfileId.HasValue)
        {
            // TODO: check permission to represent profiles
            isRequirementMet = true;
        }
        else
        {
            isRequirementMet = true;
        }

        if (isRequirementMet)
        {
            context.Succeed(requirement);
        }
        else
        {
            this.httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }

        return Task.CompletedTask;
    }
}
