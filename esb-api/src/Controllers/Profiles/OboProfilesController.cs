using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/obo/profiles")]
public class OboProfilesController : ControllerBase
{
    /// <summary>
    /// Връща профил, който може да получава съобщения, по даден идентификатор и шаблон на съобщението
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboProfilesAccess)]
    [HttpGet("search")]
    [ProducesResponseType(typeof(ProfileDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery, BindRequired] string identifier,
        [FromQuery] int? templateId,
        [FromQuery, BindRequired] int targetGroupId,
        CancellationToken ct)
    {
        DomainServices.Esb.SearchTargetGroupProfilesResponse resp =
            await esbClient.SearchTargetGroupProfilesAsync(
                new DomainServices.Esb.SearchTargetGroupProfilesRequest
                {
                    Identifier = identifier,
                    TemplateId = templateId,
                    TargetGroupId = targetGroupId,
                },
                cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        return this.Ok(resp.Result.Adapt<ProfileDO>());
    }
}
