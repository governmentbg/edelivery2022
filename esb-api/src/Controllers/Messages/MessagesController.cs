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
using Newtonsoft.Json.Linq;
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
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(TableResultDO<InboxDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInboxAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? templateId,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.InboxResponse resp =
            await esbClient.InboxAsync(
                new DomainServices.Esb.BoxRequest
                {
                    ProfileId = profileId,
                    From = from?.ToTimestamp(),
                    To = to?.ToTimestamp(),
                    TemplateId = templateId,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        return this.Ok(resp.Adapt<TableResultDO<InboxDO>>());
    }

    /// <summary>
    /// Връща списък с изпратени съобщения
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("outbox")]
    [ProducesResponseType(typeof(TableResultDO<OutboxDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOutboxAsync(
        [FromServices] EsbClient esbClient,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? templateId,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.OutboxResponse resp =
            await esbClient.OutboxAsync(
                new DomainServices.Esb.BoxRequest
                {
                    ProfileId = profileId,
                    From = from?.ToTimestamp(),
                    To = to?.ToTimestamp(),
                    TemplateId = templateId,
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
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.SendMessage)]
    [HttpPost("")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] ITemplateService templateService,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromBody] MessageSendDO message,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();
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
                    SenderProfileId = profileId,
                    SenderLoginId = loginId,
                    SenderViaLoginId = null,
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

    /// <summary>
    /// Изпращане на съобщение с код
    /// </summary>
    /// <returns>Публичен идентификатор на изпратеното съобщение</returns>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [ApiExplorerSettings(IgnoreApi = true)]
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Преглед на получено съобщение
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadMessageAsRecipient)]
    [HttpGet("{messageId:int}/open")]
    [ProducesResponseType(typeof(MessageOpenDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] ITemplateService templateService,
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

        IList<BaseComponent> components =
          await templateService.GetTemplateComponentsAsync(
              result.TemplateId,
              ct);

        Dictionary<Guid, object?> parsedMessageBody =
            JsonConvert.DeserializeObject<Dictionary<Guid, object?>>(
                resp.Message.Body,
                new MessageBodyConverter(components))
                    ?? throw new Exception("Invalid message body");

        Dictionary<Guid, object?> fields = new();

        foreach (var item in parsedMessageBody)
        {
            if (item.Value is FileObject[] fos)
            {
                MessageOpenDOBlob[] blobs = fos
                    .Select(f =>
                    {
                        DomainServices.Esb.GetMessageResponse.Types.Blob blob =
                            resp.Message.Blobs.FirstOrDefault(e => e.BlobId == f.FileId)
                            ?? resp.Message.Blobs.First(e =>
                                f.FileName.ToUpperInvariant().StartsWith(e.FileName.ToUpperInvariant())
                                    && f.FileHash.ToUpperInvariant().EndsWith(e.Hash.ToUpperInvariant()));

                        (string link, DateTime expirationDate) =
                            blobUrlCreator.CreateMessageBlobUrl(
                                profileId,
                                messageId,
                                blob.BlobId);

                        MessageOpenDOBlob result = blob.Adapt<MessageOpenDOBlob>() with
                        {
                            DownloadLink = link,
                            DownloadLinkExpirationDate = expirationDate,
                        };

                        return result;
                    })
                    .ToArray();

                fields.Add(item.Key, blobs);
            }
            else
            {
                fields.Add(item.Key, item.Value);
            }
        }

        if (result.DateReceived.HasValue)
        {
            return this.Ok(result with
            {
                Fields = fields,
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
        });
    }

    /// <summary>
    /// Преглед на получено препратено съобщение
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadForwardedMessageAsRecipient)]
    [HttpGet("{messageId:int}/open-forwarded/{forwardedMessageId:int}")]
    [ProducesResponseType(typeof(ForwardedMessageOpenDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> OpenForwardedAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] ITemplateService templateService,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int messageId,
        [FromRoute] int forwardedMessageId,
        CancellationToken ct)
    {
        DomainServices.Esb.GetForwardedMessageOriginalRecipientProfileResponse recipientResp = 
            await esbClient.GetForwardedMessageOriginalRecipientProfileAsync(
                new DomainServices.Esb.GetForwardedMessageOriginalRecipientProfileRequest
                {
                    MessageId = messageId,
                    ForwardedMessageId = forwardedMessageId,
                },
                cancellationToken: ct);

        int profileId = recipientResp.RecipientProfileId;

        DomainServices.Esb.GetMessageResponse resp =
           await esbClient.GetMessageAsync(
               new DomainServices.Esb.GetMessageRequest
               {
                   MessageId = forwardedMessageId,
                   ProfileId = profileId,
               },
               cancellationToken: ct);

        ForwardedMessageOpenDO result = resp.Message.Adapt<ForwardedMessageOpenDO>();

        IList<BaseComponent> components =
          await templateService.GetTemplateComponentsAsync(
              result.TemplateId,
              ct);

        Dictionary<Guid, object?> parsedMessageBody =
            JsonConvert.DeserializeObject<Dictionary<Guid, object?>>(
                resp.Message.Body,
                new MessageBodyConverter(components))
                    ?? throw new Exception("Invalid message body");

        Dictionary<Guid, object?> fields = new();

        foreach (var item in parsedMessageBody)
        {
            if (item.Value is FileObject[] fos)
            {
                ForwardedMessageOpenDOBlob[] blobs = fos
                    .Select(f =>
                    {
                        DomainServices.Esb.GetMessageResponse.Types.Blob blob =
                            resp.Message.Blobs.FirstOrDefault(e => e.BlobId == f.FileId)
                            ?? resp.Message.Blobs.First(e =>
                                f.FileName.ToUpperInvariant().StartsWith(e.FileName.ToUpperInvariant())
                                    && f.FileHash.ToUpperInvariant().EndsWith(e.Hash.ToUpperInvariant()));

                        (string link, DateTime expirationDate) =
                            blobUrlCreator.CreateMessageBlobUrl(
                                profileId,
                                forwardedMessageId,
                                blob.BlobId);

                        ForwardedMessageOpenDOBlob result = blob.Adapt<ForwardedMessageOpenDOBlob>() with
                        {
                            DownloadLink = link,
                            DownloadLinkExpirationDate = expirationDate,
                        };

                        return result;
                    })
                    .ToArray();

                fields.Add(item.Key, blobs);
            }
            else
            {
                fields.Add(item.Key, item.Value);
            }
        }

        if (result.DateReceived.HasValue)
        {
            return this.Ok(result with
            {
                Fields = fields,
            });
        }
        else
        {
            throw new Exception("Can not open forwarded message");
        }
    }

    /// <summary>
    /// Преглед на изпратено съобщение
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.ReadMessageAsSender)]
    [HttpGet("{messageId:int}/view")]
    [ProducesResponseType(typeof(MessageViewDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ViewAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] ITemplateService templateService,
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

        IList<BaseComponent> components =
           await templateService.GetTemplateComponentsAsync(
               result.TemplateId,
               ct);

        Dictionary<Guid, object?> parsedMessageBody =
            JsonConvert.DeserializeObject<Dictionary<Guid, object?>>(
                resp.Message.Body,
                new MessageBodyConverter(components))
                    ?? throw new Exception("Invalid message body");

        Dictionary<Guid, object?> fields = new();

        foreach (var item in parsedMessageBody)
        {
            if (item.Value is FileObject[] fos)
            {
                MessageViewDOBlob[] blobs = fos
                    .Select(f =>
                    {
                        DomainServices.Esb.ViewMessageResponse.Types.Blob blob =
                            resp.Message.Blobs.FirstOrDefault(e => e.BlobId == f.FileId)
                            ?? resp.Message.Blobs.First(e =>
                                f.FileName.ToUpperInvariant().StartsWith(e.FileName.ToUpperInvariant())
                                    && f.FileHash.ToUpperInvariant().EndsWith(e.Hash.ToUpperInvariant()));

                        (string link, DateTime expirationDate) =
                            blobUrlCreator.CreateMessageBlobUrl(
                                profileId,
                                messageId,
                                blob.BlobId);

                        MessageViewDOBlob result = blob.Adapt<MessageViewDOBlob>() with
                        {
                            DownloadLink = link,
                            DownloadLinkExpirationDate = expirationDate,
                        };

                        return result;
                    })
                    .ToArray();

                fields.Add(item.Key, blobs);
            }
            else
            {
                fields.Add(item.Key, item.Value);
            }
        }

        return this.Ok(result with
        {
            Fields = fields
        });
    }
}
