using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using static ED.DomainServices.Authorization;

namespace ED.EsbApi;

public class SendMessageRequirement : IAuthorizationRequirement
{
}

public class SendMessageRequirementHandler : AuthorizationHandler<SendMessageRequirement>
{
    private readonly HttpContext httpContext;
    private readonly AuthorizationClient authorizationClient;

    public SendMessageRequirementHandler(
        IHttpContextAccessor httpContextAccessor,
        AuthorizationClient authorizationClient)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            throw new Exception($"'{nameof(SendMessageRequirementHandler)}' called outside of a request. IHttpContextAccessor.HttpContext is null!");
        }

        this.httpContext = httpContextAccessor.HttpContext;
        this.authorizationClient = authorizationClient;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SendMessageRequirement requirement)
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

        //https://github.com/alicommit-malp/Dotnet-Core-Request-Body-Peeker/blob/d18dc111c88dcf339b003ac93eea46652ea6d7ff/src/Request.Body.Peeker/HttpRequestExtension.cs
        this.httpContext.Request.EnableBuffering();
        Memory<byte> memory = new byte[Convert.ToInt32(this.httpContext.Request.ContentLength)];
        await this.httpContext.Request.Body.ReadAsync(memory);
        string body = new UTF8Encoding().GetString(memory.ToArray());
        this.httpContext.Request.Body.Position = 0;

        string? templateParam = JObject.Parse(body)["templateId"]?.Value<string>();
        int templateId = int.TryParse(templateParam, out int temp) ? temp : 0;

        DomainServices.HasAccessResponse resp =
            await this.authorizationClient.HasWriteMessageAccessAsync(
                new DomainServices.HasWriteMessageAccessRequest
                {
                    ProfileId = profileId.Value,
                    LoginId = loginId.Value,
                    TemplateId = templateId,
                });

        if (!resp.HasAccess)
        {
            this.httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        context.Succeed(requirement);
        return;
    }
}
