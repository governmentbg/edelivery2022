using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

public class OboProfilesAccessRequirement : IAuthorizationRequirement
{
}

public class OboProfilesAccessRequirementHandler : AuthorizationHandler<OboProfilesAccessRequirement>
{
    private readonly HttpContext httpContext;
    private readonly EsbClient esbClient;

    public OboProfilesAccessRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        EsbClient esbClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(OboProfilesAccessRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.esbClient = esbClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OboProfilesAccessRequirement requirement)
    {
        int profileId = context.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.CheckProfileOnBehalfOfAccessResponse resp =
            await this.esbClient.CheckProfileOnBehalfOfAccessAsync(
                new DomainServices.Esb.CheckProfileOnBehalfOfAccessRequest
                {
                    ProfileId = profileId,
                });

        if (resp.HasAccess)
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
