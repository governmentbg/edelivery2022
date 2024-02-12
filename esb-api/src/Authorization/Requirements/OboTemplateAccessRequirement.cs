using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

public class OboTemplateAccessRequirement : IAuthorizationRequirement
{
}

public class OboTemplateAccessRequirementHandler : AuthorizationHandler<OboTemplateAccessRequirement>
{
    private readonly HttpContext httpContext;
    private readonly EsbClient esbClient;

    public OboTemplateAccessRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        EsbClient esbClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(OboTemplateAccessRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.esbClient = esbClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OboTemplateAccessRequirement requirement)
    {
        int? representedProfileId = this.httpContext.User.GetAuthenticatedUserRepresentedProfileIdOrDefault();
        if (!representedProfileId.HasValue)
        {
            context.Fail();
            return;
        }

        string? templateIdParam = this.httpContext.GetFromRouteOrQuery("templateId");
        bool isRequirementMet = false;

        if (int.TryParse(templateIdParam, out int templateId))
        {
            DomainServices.Esb.CheckProfileTemplateAccessResponse resp =
                await this.esbClient.CheckProfileTemplateAccessAsync(
                    new DomainServices.Esb.CheckProfileTemplateAccessRequest
                    {
                        ProfileId = representedProfileId.Value,
                        TemplateId = templateId,
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
