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
    /// Проверка на регистрирани профили. Връща списък от профили в различните целеви групи, отговарящи на критериите.
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("check")]
    [ProducesResponseType(typeof(TableResultDO<RegisteredProfileDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery, BindRequired] string identifier,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        DomainServices.Esb.GetRegisteredProfilesResponse resp =
            await esbClient.GetRegisteredProfilesAsync(
                new DomainServices.Esb.GetRegisteredProfilesRequest
                {
                    Identifier = identifier,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        return this.Ok(resp.Adapt<TableResultDO<RegisteredProfileDO>>());
    }

    /// <summary>
    /// Връща списък с активните профили в целева група
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesTargetGroupAccess)]
    [HttpGet("")]
    [ProducesResponseType(typeof(TableResultDO<ProfileDO>), StatusCodes.Status200OK)]
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
    /// Връща информация за профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize] // todo check for target group permissions
    [HttpGet("{profileId:int}")]
    [ProducesResponseType(typeof(ProfileDetailsDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetailsAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int profileId,
        CancellationToken ct)
    {
        DomainServices.Esb.GetProfileResponse resp =
            await esbClient.GetProfileAsync(
                new DomainServices.Esb.GetProfileRequest
                {
                    ProfileId = profileId,
                },
                cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        return this.Ok(resp.Result.Adapt<ProfileDetailsDO>());
    }

    /// <summary>
    /// Връща профил, който може да получава съобщения, по даден идентификатор и шаблон на съобщението
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesTargetGroupAccess)]
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

    /// <summary>
    /// Пасивна регистрация на профил на ФЛ, с цел получаване на съобщения и нотификации
    /// </summary>
    /// <param name="profile">Данни за регистрация на профил на ФЛ</param>
    /// <param name="ct"></param>
    /// <returns>Публичен идентификатор на регистрирания профил</returns>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ProfilesIndividualTargetGroupAccess)] // todo: or just authorize?
    [HttpPost("passive-individual")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterPassiveIndividualAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] RegisterPassiveIndividualDO profile,
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
                    Address = profile.Address.Adapt<DomainServices.Esb.CreatePassiveIndividualRequest.Types.Address>(),
                    Ip = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    ActionLoginId = loginId,
                },
                cancellationToken: ct);

        return this.Ok(resp.ProfileId);
    }

    /// <summary>
    /// Регистрация на профил на ФЛ
    /// </summary>
    /// <param name="profile">Данни за регистрация на профил на ФЛ</param>
    /// <param name="ct"></param>
    /// <returns>Публичен идентификатор на регистрирания профил</returns>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(Policy = Policies.ProfilesIndividualTargetGroupAccess)] // todo: or just authorize?
    [HttpPost("individual")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterIndividualAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] RegisterIndividualDO profile,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        DomainServices.Esb.CreateOrUpdateIndividualResponse resp =
            await esbClient.CreateOrUpdateIndividualAsync(
                new DomainServices.Esb.CreateOrUpdateIndividualRequest
                {
                    FirstName = profile.FirstName,
                    MiddleName = profile.MiddleName,
                    LastName = profile.LastName,
                    Identifier = profile.Identifier,
                    Phone = profile.Phone,
                    Email = profile.Email,
                    Address = profile.Address.Adapt<DomainServices.Esb.CreateOrUpdateIndividualRequest.Types.Address>(),
                    IsEmailNotificationEnabled = profile.IsEmailNotificationEnabled,
                    IsFullFeatured = profile.IsFullFeatured,
                    Ip = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    ActionLoginId = loginId,
                },
                cancellationToken: ct);

        return this.Ok(resp.ProfileId);
    }

    /// <summary>
    /// Регистрация на профил на ЮЛ
    /// </summary>
    /// <param name="profile">Данни за регистрация на профил на ЮЛ</param>
    /// <param name="ct"></param>
    /// <returns>Публичен идентификатор на регистрирания профил</returns>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize] // todo register any legal entity?
    [HttpPost("legal-entity")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterLegalEntityAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] RegisterLegalEntityDO profile,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        DomainServices.Esb.CreateLegalEntityResponse resp =
            await esbClient.CreateLegalEntityAsync(
                new DomainServices.Esb.CreateLegalEntityRequest
                {
                    Identifier = profile.Identifier,
                    Name = profile.Name,
                    Email = profile.Email,
                    Phone = profile.Phone,
                    Address = profile.Address.Adapt<DomainServices.Esb.CreateLegalEntityRequest.Types.Address>(),
                    TargetGroupId = profile.TargetGroupId,
                    OwnersData =
                    {
                        profile.OwnersData.ProjectToType<DomainServices.Esb.CreateLegalEntityRequest.Types.OwnerData>()
                    },
                    ActionLoginId = loginId,
                    Ip = this.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                },
                cancellationToken: ct);

        return this.Ok(resp.ProfileId);
    }
}
