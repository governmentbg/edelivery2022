using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

public class ProfilesTargetGroupAccessRequirement : IAuthorizationRequirement
{
    public int? TargetGroupId { get; set; }

    public ProfilesTargetGroupAccessRequirement(int? targetGroupId)
    {
        this.TargetGroupId = targetGroupId;
    }
}

public class ProfilesTargetGroupAccessRequirementHandler : AuthorizationHandler<ProfilesTargetGroupAccessRequirement>
{
    private readonly HttpContext httpContext;
    private readonly EsbClient esbClient;

    public ProfilesTargetGroupAccessRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        EsbClient esbClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(ProfilesTargetGroupAccessRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.esbClient = esbClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProfilesTargetGroupAccessRequirement requirement)
    {
        int? profileId = context.User.GetAuthenticatedUserProfileIdOrDefault();
        if (!profileId.HasValue)
        {
            context.Fail();
            return;
        }

        string? targetGroupIdParam = this.httpContext.GetFromRouteOrQuery("targetGroupId");
        int targetGroupId;

        if (requirement.TargetGroupId.HasValue)
        {
            targetGroupId = requirement.TargetGroupId.Value;
        }
        else
        {
            if (!int.TryParse(targetGroupIdParam, out targetGroupId))
            {
                this.httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        DomainServices.Esb.CheckProfileTargetGroupAccessResponse resp =
            await this.esbClient.CheckProfileTargetGroupAccessAsync(
                new DomainServices.Esb.CheckProfileTargetGroupAccessRequest
                {
                    ProfileId = profileId.Value,
                    TargetGroupId = targetGroupId,
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
