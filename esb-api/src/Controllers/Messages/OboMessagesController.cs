using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/obo/messages")]
public class OboMessagesController : ControllerBase
{
    /// <summary>
    /// Връща списък с получени съобщения от името на профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboReadInbox)]
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(TableResultDO<InboxDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInboxAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();
        int? representedProfileId = this.HttpContext.User.GetAuthenticatedUserRepresentedProfileId();

        DomainServices.Esb.InboxResponse resp =
            await esbClient.InboxAsync(
                new DomainServices.Esb.BoxRequest
                {
                    ProfileId = representedProfileId ?? profileId,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        return this.Ok(resp.Adapt<TableResultDO<InboxDO>>());
    }

    /// <summary>
    /// Връща списък с изпратени съобщения от името на профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboReadOutbox)]
    [HttpGet("outbox")]
    [ProducesResponseType(typeof(TableResultDO<OutboxDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOutboxAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();
        int? representedProfileId = this.HttpContext.User.GetAuthenticatedUserRepresentedProfileId();

        DomainServices.Esb.OutboxResponse resp =
            await esbClient.OutboxAsync(
                new DomainServices.Esb.BoxRequest
                {
                    ProfileId = representedProfileId ?? profileId,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        return this.Ok(resp.Adapt<TableResultDO<OutboxDO>>());
    }

    /// <summary>
    /// Изпращане на съобщение от името на чужд профил
    /// </summary>
    /// <returns>Публичен идентификатор на изпратеното съобщение</returns>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    // TODO: authorization
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] ITemplateService templateService,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] MessageSendOnBehalfOfDO message,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        IList<BaseComponent> components =
            await templateService.GetTemplateComponentsAsync(
                message.TemplateId,
                ct);

        IEnumerable<Guid> fileComponentGuids = components
           .Where(e => e.Type == ComponentType.file)
           .Select(e => e.Id);

        int[] blobIds = message.Fields
            .Where(e => fileComponentGuids.Contains(e.Key) && e.Value != null)
            .Select(e => ((JArray)e.Value!).ToObject<int[]>()!)
            .SelectMany(e => e)
            .Distinct()
            .ToArray();

        DomainServices.Esb.GetBlobsInfoResponse respBlobsInfo =
            await esbClient.GetBlobsInfoAsync(
                new DomainServices.Esb.GetBlobsInfoRequest
                {
                    BlobIds =
                    {
                        blobIds
                    }
                },
                cancellationToken: ct);

        (string body, string metaFields) =
            SystemTemplateUtils.GetNewMessageBodyJson(
                components,
                message.Fields,
                respBlobsInfo.Result.ToArray());

        DomainServices.Esb.SendMessageResponse resp =
            await esbClient.SendMessageAsync(
                new DomainServices.Esb.SendMessageRequest
                {
                    RecipientProfileIds =
                    {
                        message.RecipientProfileIds
                    },
                    SenderProfileId = message.SenderProfileId,
                    SenderLoginId = loginId,
                    SenderViaLoginId = loginId,
                    TemplateId = message.TemplateId,
                    Subject = message.Subject,
                    Rnu = message.Rnu,
                    Body = body,
                    MetaFields = metaFields,
                    BlobIds = { blobIds },
                    ForwardedMessageId = null,
                },
                cancellationToken: ct);

        return this.Ok(resp.MessageId);
    }
}
