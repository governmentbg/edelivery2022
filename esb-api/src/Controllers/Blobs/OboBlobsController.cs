using System;
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
[Route("api/v{version:apiVersion}/obo/blobs")]
public class OboBlobsController : ControllerBase
{
    /// <summary>
    /// Връща списък с файлове в хранилището от името на профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboStorageAccess)]
    [HttpGet("")]
    [ProducesResponseType(typeof(TableResultDO<BlobDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListOnBehalfAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        int? representedProfileId = this.HttpContext.User.GetAuthenticatedUserRepresentedProfileId();

        DomainServices.Esb.GetStorageBlobsResponse resp =
            await esbClient.GetStorageBlobsAsync(
                new DomainServices.Esb.GetStorageBlobsRequest
                {
                    ProfileId = representedProfileId!.Value,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        BlobDO[] blobs = new BlobDO[resp.Result.Count];

        for (int i = 0; i < resp.Result.Count; i++)
        {
            var currentBlob = resp.Result[i];
            (string link, DateTime expirationDate) =
                blobUrlCreator.CreateProfileBlobUrl(representedProfileId.Value, currentBlob.BlobId);

            blobs[i] = currentBlob.Adapt<BlobDO>() with
            {
                DownloadLink = link,
                DownloadLinkExpirationDate = expirationDate,
            };
        }

        TableResultDO<BlobDO> result = new(blobs, resp.Length);

        return this.Ok(result);
    }

    /// <summary>
    /// Връща файл от хранилището от името на профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboStorageAccess)]
    [HttpGet("{blobId:int}")]
    [ProducesResponseType(typeof(BlobDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetailsOnBehalfAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int blobId,
        CancellationToken ct)
    {
        int? representedProfileId = this.HttpContext.User.GetAuthenticatedUserRepresentedProfileId();

        DomainServices.Esb.GetStorageBlobInfoResponse resp =
            await esbClient.GetStorageBlobInfoAsync(
                new DomainServices.Esb.GetStorageBlobInfoRequest
                {
                    ProfileId = representedProfileId!.Value,
                    BlobId = blobId,
                },
                cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        (string link, DateTime expirationDate) =
                blobUrlCreator.CreateProfileBlobUrl(representedProfileId.Value, resp.Result.BlobId);

        BlobDO result = resp.Result.Adapt<BlobDO>() with
        {
            DownloadLink = link,
            DownloadLinkExpirationDate = expirationDate,
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Изтрива файл от хранилището от името на профил
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize(Policy = Policies.OboStorageAccess)]
    [HttpDelete("{blobId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOnBehalfAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int blobId,
        CancellationToken ct)
    {
        int? representedProfileId = this.HttpContext.User.GetAuthenticatedUserRepresentedProfileId();

        DomainServices.Esb.CheckStorageBlobResponse resp =
            await esbClient.CheckStorageBlobAsync(
                new DomainServices.Esb.CheckStorageBlobRequest
                {
                    ProfileId = representedProfileId!.Value,
                    BlobId = blobId,
                },
                cancellationToken: ct);

        if (!resp.IsThere)
        {
            return this.NotFound();
        }

        _ = await esbClient.DeteleStorageBlobAsync(
            new DomainServices.Esb.DeteleStorageBlobRequest
            {
                ProfileId = representedProfileId!.Value,
                BlobId = blobId,
            },
            cancellationToken: ct);

        return this.Ok();
    }
}
