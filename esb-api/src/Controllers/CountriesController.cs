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
[Route("api/v{version:apiVersion}/countries")]
public class CountriesController : ControllerBase
{
    /// <summary>
    /// Връща списък с всички страни като номенклатура
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(typeof(TableResultDO<CountryDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        DomainServices.Esb.GetCountriesResponse resp =
            await esbClient.GetCountriesAsync(
                new DomainServices.Esb.GetCountriesRequest
                {
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        return this.Ok(resp.Adapt<TableResultDO<CountryDO>>());
    }
}
