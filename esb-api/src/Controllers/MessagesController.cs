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
using Newtonsoft.Json;
using static ED.DomainServices.Esb.Esb;

namespace ED.EsbApi;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/messages")]
public class MessagesController : ControllerBase
{
    /// <summary>
    /// Връща списък с получени съобщения
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadInbox)]
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
    /// Връща списък с изпратени съобщения
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadOutbox)]
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
    /// Изпращане на съобщение
    /// </summary>
    /// <returns>Публичен идентификатор на изпратеното съобщение</returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.SendMessage)]
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] MessageSendDO message,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        DomainServices.Esb.GetTemplateResponse respTemplate =
           await esbClient.GetTemplateAsync(
               new DomainServices.Esb.GetTemplateRequest
               {
                   TemplateId = message.TemplateId,
               },
               cancellationToken: ct);

        List<BaseComponent> components =
            JsonConvert.DeserializeObject<List<BaseComponent>>(
                respTemplate.Result.Content,
                new TemplateComponentConverter())
                ?? new List<BaseComponent>();

        int[] blobIds = message.Blobs
            .SelectMany(e => e.Value)
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

        SystemTemplateUtils.BlobInfo[] blobsInfo =
            new SystemTemplateUtils.BlobInfo[blobIds.Length];

        for (int i = 0; i < blobsInfo.Length; i++)
        {
            int blobId = blobIds[i];

            Guid matchFieldId = message.Blobs
                .First(e => e.Value.Contains(blobId))
                .Key;

            var matchBlob = respBlobsInfo.Result.First(e => e.BlobId == blobId);

            blobsInfo[i] = new SystemTemplateUtils.BlobInfo(
                matchFieldId,
                matchBlob.FileName,
                matchBlob.HashAlgorithm,
                matchBlob.Hash, Convert.ToUInt64(matchBlob.Size));
        }

        (string body, string metaFields) =
            SystemTemplateUtils.GetNewMessageBodyJson(
                components,
                message.Fields,
                blobsInfo);

        DomainServices.Esb.SendMessageResponse resp =
            await esbClient.SendMessageAsync(
                new DomainServices.Esb.SendMessageRequest
                {
                    RecipientProfileIds =
                    {
                        message.RecipientProfileIds
                    },
                    SenderProfileId = profileId,
                    SenderLoginId = loginId,
                    SenderViaLoginId = null,
                    TemplateId = message.TemplateId,
                    Subject = message.Subject,
                    ReferencedOrn = message.ReferencedOrn,
                    AdditionalIdentifier = message.AdditionalIdentifier,
                    Body = body,
                    MetaFields = metaFields,
                    BlobIds = { blobIds },
                    ForwardedMessageId = message.ForwardedMessageId,
                },
                cancellationToken: ct);

        return this.Ok(resp.MessageId);
    }

    /// <summary>
    /// Изпращане на съобщение от името на чужд профил
    /// </summary>
    /// <returns>Публичен идентификатор на изпратеното съобщение</returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    // TODO: authorization
    [HttpPost("on-behalf")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendOnBehalfAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] MessageSendOnBehalfOfDO message,
        CancellationToken ct)
    {
        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        DomainServices.Esb.GetTemplateResponse respTemplate =
           await esbClient.GetTemplateAsync(
               new DomainServices.Esb.GetTemplateRequest
               {
                   TemplateId = message.TemplateId,
               },
               cancellationToken: ct);

        List<BaseComponent> components =
            JsonConvert.DeserializeObject<List<BaseComponent>>(
                respTemplate.Result.Content,
                new TemplateComponentConverter())
                ?? new List<BaseComponent>();

        int[] blobIds = message.Blobs
            .SelectMany(e => e.Value)
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

        SystemTemplateUtils.BlobInfo[] blobsInfo =
            new SystemTemplateUtils.BlobInfo[blobIds.Length];

        for (int i = 0; i < blobsInfo.Length; i++)
        {
            int blobId = blobIds[i];

            Guid matchFieldId = message.Blobs
                .First(e => e.Value.Contains(blobId))
                .Key;

            var matchBlob = respBlobsInfo.Result.First(e => e.BlobId == blobId);

            blobsInfo[i] = new SystemTemplateUtils.BlobInfo(
                matchFieldId,
                matchBlob.FileName,
                matchBlob.HashAlgorithm,
                matchBlob.Hash, Convert.ToUInt64(matchBlob.Size));
        }

        (string body, string metaFields) =
            SystemTemplateUtils.GetNewMessageBodyJson(
                components,
                message.Fields,
                blobsInfo);

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
                    ReferencedOrn = message.ReferencedOrn,
                    AdditionalIdentifier = message.AdditionalIdentifier,
                    Body = body,
                    MetaFields = metaFields,
                    BlobIds = { blobIds },
                    ForwardedMessageId = null,
                },
                cancellationToken: ct);

        return this.Ok(resp.MessageId);
    }

    /// <summary>
    /// Изпращане на съобщение с код
    /// </summary>
    /// <returns>Публичен идентификатор на изпратеното съобщение</returns>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [HttpPost("code")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<int> SendCodeAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] CodeMessageSendDO message,
        CancellationToken ct)
    {
        // TODO: mind login-profile permissions

        throw new NotImplementedException();
    }

    /// <summary>
    /// Преглед на получено съобщение
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadMessageAsRecipient)]
    [HttpGet("{messageId:int}/open")]
    [ProducesResponseType(typeof(MessageOpenDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int messageId,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.GetMessageResponse resp =
           await esbClient.GetMessageAsync(
               new DomainServices.Esb.GetMessageRequest
               {
                   MessageId = messageId,
                   ProfileId = profileId,
               },
               cancellationToken: ct);

        MessageOpenDO result = resp.Message.Adapt<MessageOpenDO>();

        Dictionary<Guid, string?> fields =
            JsonExtensions.ParseMessageBody(resp.Message.Body);

        MessageOpenDOBlob[] newBlobs = new MessageOpenDOBlob[result.Blobs.Length];
        for (int i = 0; i < result.Blobs.Length; i++)
        {
            (string link, DateTime expirationDate) =
                blobUrlCreator.CreateMessageBlobUrl(
                    profileId,
                    messageId,
                    result.Blobs[i].BlobId);

            newBlobs[i] = result.Blobs[i] with
            {
                DownloadLink = link,
                DownloadLinkExpirationDate = expirationDate,
            };
        }

        if (result.DateReceived.HasValue)
        {
            return this.Ok(result with
            {
                Fields = fields,
                Blobs = newBlobs
            });
        }

        int loginId = this.HttpContext.User.GetAuthenticatedUserLoginId();

        DomainServices.Esb.OpenMessageResponse openResp =
            await esbClient.OpenMessageAsync(
                new DomainServices.Esb.OpenMessageRequest
                {
                    ProfileId = profileId,
                    MessageId = messageId,
                    LoginId = loginId,
                },
                cancellationToken: ct);

        if (openResp.Result == null)
        {
            throw new Exception("Can not open message");
        }

        return this.Ok(result with
        {
            Fields = fields,
            DateReceived = openResp.Result.DateReceived.ToLocalDateTime(),
            RecipientLogin = new MessageOpenDORecipientLogin(
                openResp.Result.LoginId,
                openResp.Result.LoginName),
            Blobs = newBlobs
        });
    }

    /// <summary>
    /// Преглед на изпратено съобщение
    /// </summary>
    /// <include file='../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadMessageAsSender)]
    [HttpGet("{messageId:int}/view")]
    [ProducesResponseType(typeof(MessageViewDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ViewAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int messageId,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.ViewMessageResponse resp =
           await esbClient.ViewMessageAsync(
               new DomainServices.Esb.ViewMessageRequest
               {
                   MessageId = messageId,
               },
               cancellationToken: ct);

        MessageViewDO result = resp.Message.Adapt<MessageViewDO>();

        Dictionary<Guid, string?> fields =
            JsonExtensions.ParseMessageBody(resp.Message.Body);

        MessageViewDOBlob[] newBlobs = new MessageViewDOBlob[result.Blobs.Length];
        for (int i = 0; i < result.Blobs.Length; i++)
        {
            (string link, DateTime expirationDate) =
                blobUrlCreator.CreateMessageBlobUrl(
                    profileId,
                    messageId,
                    result.Blobs[i].BlobId);

            newBlobs[i] = result.Blobs[i] with
            {
                DownloadLink = link,
                DownloadLinkExpirationDate = expirationDate,
            };
        }

        return this.Ok(result with
        {
            Fields = fields,
            Blobs = newBlobs
        });
    }
}
