using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/target-groups")]
public class TargetGroupsController : ControllerBase
{
    /// <summary>
    /// Връща списък с всички активни целеви групи
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(typeof(List<TargetGroupDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.GetTargetGroupsResponse resp =
            await esbClient.GetTargetGroupsAsync(
                new DomainServices.Esb.GetTargetGroupsRequest
                {
                    ProfileId = profileId,
                },
                cancellationToken: ct);

        return this.Ok(resp.Result.ProjectToType<TargetGroupDO>());
    }
}
