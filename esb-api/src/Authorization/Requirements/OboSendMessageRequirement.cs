using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using static ED.DomainServices.Authorization;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

public class OboSendMessageRequirement : IAuthorizationRequirement
{
}

public class OboSendMessageRequirementHandler : AuthorizationHandler<OboSendMessageRequirement>
{
    private readonly HttpContext httpContext;
    private readonly AuthorizationClient authorizationClient;
    private readonly EsbClient esbClient;

    public OboSendMessageRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        AuthorizationClient authorizationClient,
        EsbClient esbClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(OboSendMessageRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.authorizationClient = authorizationClient;
        this.esbClient = esbClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OboSendMessageRequirement requirement)
    {
        int? representedProfileId = context.User.GetAuthenticatedUserRepresentedProfileIdOrDefault();
        if (!representedProfileId.HasValue)
        {
            context.Fail();
            return;
        }

        int? profileId = context.User.GetAuthenticatedUserProfileIdOrDefault();
        if (!profileId.HasValue)
        {
            context.Fail();
            return;
        }

        DomainServices.Esb.CheckProfileOnBehalfOfAccessResponse resp =
            await this.esbClient.CheckProfileOnBehalfOfAccessAsync(
                new DomainServices.Esb.CheckProfileOnBehalfOfAccessRequest
                {
                    ProfileId = profileId.Value,
                });

        if (!resp.HasAccess)
        {
            context.Fail();
            return;
        }

        //https://github.com/alicommit-malp/Dotnet-Core-Request-Body-Peeker/blob/d18dc111c88dcf339b003ac93eea46652ea6d7ff/src/Request.Body.Peeker/HttpRequestExtension.cs
        this.httpContext.Request.EnableBuffering();
        Memory<byte> memory = new byte[Convert.ToInt32(this.httpContext.Request.ContentLength)];
        await this.httpContext.Request.Body.ReadAsync(memory);
        string body = new UTF8Encoding().GetString(memory.ToArray());
        this.httpContext.Request.Body.Position = 0;

        string? templateParam = JObject.Parse(body)["templateId"]?.Value<string>();
        int templateId = int.TryParse(templateParam, out int temp) ? temp : 0;

        int? operatorLoginId = context.User.GetAuthenticatedUserOperatorLoginIdOrDefault();

        if (operatorLoginId.HasValue)
        {
            DomainServices.HasAccessResponse resp2 =
                await this.authorizationClient.HasWriteMessageAccessAsync(
                    new DomainServices.HasWriteMessageAccessRequest
                    {
                        ProfileId = representedProfileId.Value,
                        LoginId = operatorLoginId.Value,
                        TemplateId = templateId,
                    });

            if (!resp2.HasAccess)
            {
                this.httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }
        else
        {
            DomainServices.Esb.CheckProfileTemplateAccessResponse resp3 =
                await this.esbClient.CheckProfileTemplateAccessAsync(
                    new DomainServices.Esb.CheckProfileTemplateAccessRequest
                    {
                        ProfileId = representedProfileId.Value,
                        TemplateId = templateId,
                    });

            if (!resp3.HasAccess)
            {
                this.httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }

        context.Succeed(requirement);
        return;
    }
}
