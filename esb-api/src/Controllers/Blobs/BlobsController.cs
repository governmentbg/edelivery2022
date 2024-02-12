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
[Route("api/v{version:apiVersion}/blobs")]
public class BlobsController : ControllerBase
{
    /// <summary>
    /// Връща списък с файлове в хранилището
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("")]
    [ProducesResponseType(typeof(TableResultDO<BlobDO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.GetStorageBlobsResponse resp =
            await esbClient.GetStorageBlobsAsync(
                new DomainServices.Esb.GetStorageBlobsRequest
                {
                    ProfileId = profileId,
                    Offset = offset,
                    Limit = limit,
                },
                cancellationToken: ct);

        BlobDO[] blobs = new BlobDO[resp.Result.Count];

        for (int i = 0; i < resp.Result.Count; i++)
        {
            var currentBlob = resp.Result[i];
            (string link, DateTime expirationDate) =
                blobUrlCreator.CreateProfileBlobUrl(profileId, currentBlob.BlobId);

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
    /// Връща файл от хранилището
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpGet("{blobId:int}")]
    [ProducesResponseType(typeof(BlobDO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DetailsAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int blobId,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.GetStorageBlobInfoResponse resp =
            await esbClient.GetStorageBlobInfoAsync(
                new DomainServices.Esb.GetStorageBlobInfoRequest
                {
                    ProfileId = profileId,
                    BlobId = blobId,
                },
                cancellationToken: ct);

        if (resp.Result == null)
        {
            return this.NotFound();
        }

        (string link, DateTime expirationDate) =
                blobUrlCreator.CreateProfileBlobUrl(profileId, resp.Result.BlobId);

        BlobDO result = resp.Result.Adapt<BlobDO>() with
        {
            DownloadLink = link,
            DownloadLinkExpirationDate = expirationDate,
        };

        return this.Ok(result);
    }

    /// <summary>
    /// Изтрива файл от хранилището
    /// </summary>
    /// <include file='../../Documentation.xml' path='Documentation/CommonParams/*'/>
    [Authorize]
    [HttpDelete("{blobId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] EsbClient esbClient,
        [FromServices] BlobUrlCreator blobUrlCreator,
        [FromHeader(Name = EsbAuthSchemeConstants.DpMiscinfoHeader), BindRequired] string dpMiscinfo,
        [FromRoute] int blobId,
        CancellationToken ct)
    {
        int profileId = this.HttpContext.User.GetAuthenticatedUserProfileId();

        DomainServices.Esb.CheckStorageBlobResponse resp =
            await esbClient.CheckStorageBlobAsync(
                new DomainServices.Esb.CheckStorageBlobRequest
                {
                    ProfileId = profileId,
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
                ProfileId = profileId,
                BlobId = blobId,
            },
            cancellationToken: ct);

        return this.Ok();
    }
}
