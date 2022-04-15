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
[Route("api/v{version:apiVersion}/profiles")]
public class ProfilesController : ControllerBase
{
    /// <summary>
    /// Връща списък с активните профили в целева група
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesTargetGroupAccess)]
    [HttpGet("")]
    [ProducesResponseType(typeof(TableResultDO<ProfileDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery, BindRequired] int targetGroupId,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        DomainServices.Esb.GetTargetGroupProfilesResponse resp =
            await esbClient.GetTargetGroupProfilesAsync(
                new DomainServices.Esb.GetTargetGroupProfilesRequest
                {
                    TargetGroupId = targetGroupId,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        return this.Ok(resp.Adapt<TableResultDO<ProfileDO>>());
    }

    /// <summary>
    /// Връща профил по идентификатор (ЕГН/ЛНЧ/ЕИК) в целева група
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesTargetGroupAccess)]
    [HttpGet("search")]
    [ProducesResponseType(typeof(ProfileSearchDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchByIdentifierAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery, BindRequired] string seachProfileIdentifier,
        [FromQuery, BindRequired] int targetGroupId,
        CancellationToken ct)
    {
        DomainServices.Esb.GetTargetGroupProfileByIdentifierResponse resp =
            await esbClient.GetTargetGroupProfileByIdentifierAsync(
                new DomainServices.Esb.GetTargetGroupProfileByIdentifierRequest
                {
                    TargetGroupId = targetGroupId,
                    Identifier = seachProfileIdentifier,
                },
                cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        return this.Ok(resp.Result.Adapt<ProfileSearchDO>());
    }

    /// <summary>
    /// Връща профил по идентификатор (ЕГН/ЛНЧ/ЕИК) в целева група без ограничения, ако профила може да изпраща от името на чужд профил
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesOnBehalfOf)]
    [HttpGet("search-on-behalf")]
    [ProducesResponseType(typeof(ProfileSearchDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchOnBehalfByIdentifierAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery, BindRequired] string seachProfileIdentifier,
        [FromQuery, BindRequired] int targetGroupId,
        CancellationToken ct)
    {
        DomainServices.Esb.GetProfileByIdentifierResponse resp =
            await esbClient.GetProfileByIdentifierAsync(
                new DomainServices.Esb.GetProfileByIdentifierRequest
                {
                    Identifier = seachProfileIdentifier,
                },
                cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        return this.Ok(resp.Result.Adapt<ProfileSearchDO>());
    }

    /// <summary>
    /// Пасивна регистрация на профил на ЮЛ, с цел получаване на съобщения и нотификации
    /// </summary>
    /// <param name="profile">Данни за регистрация на профил</param>
    /// <param name="ct"></param>
    /// <returns>Публичен идентификатор на регистрирания профил</returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesIndividualTargetGroupAccess)]
    [HttpPost("register-individual")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterIndividualAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] ProfileRegisterDO profile,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        DomainServices.Esb.CreatePassiveIndividualResponse resp =
            await esbClient.CreatePassiveIndividualAsync(
                new DomainServices.Esb.CreatePassiveIndividualRequest
                {
                    FirstName = profile.FirstName,
                    MiddleName = profile.MiddleName,
                    LastName = profile.LastName,
                    Identifier = profile.Identifier,
                    Phone = profile.Phone,
                    Email = profile.Email,
                    Residence = profile.Residence,
                    Ip = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    ActionLoginId = loginId,
                },
                cancellationToken: ct);

        return this.Ok(resp.ProfileId);
    }
}
