using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/obo/templates")]
public class OboTemplatesController : ControllerBase
{
    /// <summary>
    /// Връща списък с шаблоните на съобщението от името на профил, до които профила има достъп
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboProfilesAccess)]
    [HttpGet("")]
    [ProducesResponseType(typeof(List<TemplateDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        CancellationToken ct)
    {
        int? representedProfileId = this.HttpContext.User.GetAuthenticatedUserRepresentedProfileId();

        DomainServices.Esb.GetTemplatesResponse resp =
            await esbClient.GetTemplatesAsync(
                new DomainServices.Esb.GetTemplatesRequest
                {
                    ProfileId = representedProfileId!.Value,
                },
                cancellationToken: ct);

        return this.Ok(resp.Result.ProjectToType<TemplateDO>());
    }

    /// <summary>
    /// Връща данните за шаблон на съобщение от името на профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboTemplateAccess)]
    [HttpGet("{templateId:int}")]
    [ProducesResponseType(typeof(TemplateDetailsDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetailsAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int templateId,
        CancellationToken ct)
    {
        DomainServices.Esb.GetTemplateResponse resp =
           await esbClient.GetTemplateAsync(
               new DomainServices.Esb.GetTemplateRequest
               {
                   TemplateId = templateId,
               },
               cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        List<BaseComponent> components =
            JsonConvert.DeserializeObject<List<BaseComponent>>(
                resp.Result.Content,
                new TemplateComponentConverter())
                    ?? new List<BaseComponent>();

        TemplateDetailsDO result = new(
            resp.Result.TemplateId,
            resp.Result.Name,
            resp.Result.IdentityNumber,
            (TemplateSecurityLevel)resp.Result.Read,
            (TemplateSecurityLevel)resp.Result.Write,
            components.AsReadOnly(),
            resp.Result.ResponseTemplateId);

        return this.Ok(result);
    }
}
