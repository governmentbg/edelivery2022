using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

using ED.DomainServices.Blobs;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.Storage;
using EDelivery.WebPortal.Utils.Attributes;

using EDeliveryResources;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public partial class StorageController : BaseController
    {
        public const string DownloadActionQueryStringKey = "t";

        private readonly Lazy<Blob.BlobClient> blobClient;

        public StorageController()
        {
            this.blobClient = new Lazy<Blob.BlobClient>(
                () => Grpc.GrpcClientFactory.CreateBlobClient(), isThreadSafe: false);
        }

        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleStorageHome", eLeftMenu.Storage)]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            _ = await this.blobClient.Value.DeleteProfileBlobAsync(
                new DeleteProfileBlobRequest
                {
                    LoginId = UserData.LoginId,
                    ProfileId = UserData.ActiveProfileId,
                    BlobId = id
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public async Task<ActionResult> Browse(
            string fileId,
            string allowedFileTypes,
            long maxFileSize)
        {
            string[] allowedFileExtensions = allowedFileTypes
                ?.Split(',')
                ?.Select(x => x.Trim())
                ?.Where(x => !string.IsNullOrWhiteSpace(x))
                ?.ToArray() ?? Array.Empty<string>();

            GetMyProfileBlobsResponse blobs =
                await this.blobClient.Value.GetMyProfileBlobsAsync(
                    new GetMyProfileBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        MaxFileSize = maxFileSize,
                        AllowedFileTypes = { allowedFileExtensions },
                        Offset = 0,
                        Limit = 50,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            BrowseViewModel model = new BrowseViewModel()
            {
                FileId = fileId,
                Blobs = blobs.Result.ToList(),
                AllowedFileTypes = allowedFileTypes,
                MaxFileSize = maxFileSize
            };

            if (model.Blobs.Count == 0)
            {
                ViewBag.NoMessages = StoragePage.LabelNoFiles;
            }

            return PartialView("Partials/_Browse", model);
        }

        [ChildActionOnlyOrAjax]
        [HttpGet]
        public ActionResult ListFreeBlobs()
        {
            GetProfileFreeBlobsResponse blobs =
                this.blobClient.Value.GetProfileFreeBlobs(
                    new GetProfileFreeBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.PageSize,
                        FileName = string.Empty,
                        Author = string.Empty,
                        FromDate = null,
                        ToDate = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListFreeBlobsViewModel vm =
                new ListFreeBlobsViewModel(blobs, SystemConstants.PageSize, 1);

            return PartialView("Partials/ListFreeBlobs", vm);
        }

        [HttpPost]
        public async Task<ActionResult> ListFreeBlobs(
            SearchFreeBlobsViewModel model,
            int page = 1)
        {
            GetProfileFreeBlobsResponse blobs =
                await this.blobClient.Value.GetProfileFreeBlobsAsync(
                    new GetProfileFreeBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        FileName = model.FileName ?? string.Empty,
                        Author = model.Author ?? string.Empty,
                        FromDate = model.ParsedFromDate?.ToTimestamp(),
                        ToDate = model.ParsedToDate?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListFreeBlobsViewModel vm =
                new ListFreeBlobsViewModel(model, blobs, SystemConstants.PageSize, page);

            return PartialView("Partials/ListFreeBlobs", vm);
        }

        [ChildActionOnlyOrAjax]
        [HttpGet]
        public ActionResult ListInboxBlobs()
        {
            GetProfileInboxBlobsResponse blobs =
                this.blobClient.Value.GetProfileInboxBlobs(
                    new GetProfileInboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.PageSize,
                        FileName = string.Empty,
                        MessageSubject = string.Empty,
                        FromDate = null,
                        ToDate = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListInboxBlobsViewModel vm = new ListInboxBlobsViewModel()
            {
                Blobs = new PagedList.PagedListLight<GetProfileInboxBlobsResponse.Types.Blob>(
                    new List<GetProfileInboxBlobsResponse.Types.Blob>(blobs.Result),
                    SystemConstants.PageSize,
                    1,
                    blobs.Length)
            };

            return PartialView("Partials/ListInboxBlobs", vm);
        }

        [HttpPost]
        public async Task<ActionResult> ListInboxBlobs(
            SearchInboxBlobsViewModel model,
            int page = 1)
        {
            GetProfileInboxBlobsResponse blobs =
                await this.blobClient.Value.GetProfileInboxBlobsAsync(
                    new GetProfileInboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        FileName = model.FileName ?? string.Empty,
                        MessageSubject = model.MessageSubject ?? string.Empty,
                        FromDate = model.ParsedFromDate?.ToTimestamp(),
                        ToDate = model.ParsedToDate?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListInboxBlobsViewModel vm = new ListInboxBlobsViewModel()
            {
                SearchFilter = model,
                Blobs = new PagedList.PagedListLight<GetProfileInboxBlobsResponse.Types.Blob>(
                    new List<GetProfileInboxBlobsResponse.Types.Blob>(blobs.Result),
                    SystemConstants.PageSize,
                    page,
                    blobs.Length)
            };

            return PartialView("Partials/ListInboxBlobs", vm);
        }

        [ChildActionOnlyOrAjax]
        [HttpGet]
        public ActionResult ListOutboxBlobs()
        {
            GetProfileOutboxBlobsResponse blobs =
                this.blobClient.Value.GetProfileOutboxBlobs(
                    new GetProfileOutboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.PageSize,
                        FileName = string.Empty,
                        MessageSubject = string.Empty,
                        FromDate = null,
                        ToDate = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListOutboxBlobsViewModel vm = new ListOutboxBlobsViewModel()
            {
                Blobs = new PagedList.PagedListLight<GetProfileOutboxBlobsResponse.Types.Blob>(
                    new List<GetProfileOutboxBlobsResponse.Types.Blob>(blobs.Result),
                    SystemConstants.PageSize,
                    1,
                    blobs.Length)
            };

            return PartialView("Partials/ListOutboxBlobs", vm);
        }

        [HttpPost]
        public async Task<ActionResult> ListOutboxBlobs(
            SearchOutboxBlobsViewModel model,
            int page = 1)
        {
            GetProfileOutboxBlobsResponse blobs =
                await this.blobClient.Value.GetProfileOutboxBlobsAsync(
                    new GetProfileOutboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        FileName = model.FileName ?? string.Empty,
                        MessageSubject = model.MessageSubject ?? string.Empty,
                        FromDate = model.ParsedFromDate?.ToTimestamp(),
                        ToDate = model.ParsedToDate?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListOutboxBlobsViewModel vm = new ListOutboxBlobsViewModel()
            {
                SearchFilter = model,
                Blobs = new PagedList.PagedListLight<GetProfileOutboxBlobsResponse.Types.Blob>(
                    new List<GetProfileOutboxBlobsResponse.Types.Blob>(blobs.Result),
                    SystemConstants.PageSize,
                    page,
                    blobs.Length)
            };

            return PartialView("Partials/ListOutboxBlobs", vm);
        }

        [HttpGet]
        public ActionResult DownloadProfileBlob(
            [Bind(Prefix = DownloadActionQueryStringKey)] string token)
        {
            if (!BlobUrlCreator.TryParseProfileBlobWebPortalToken(
                this.HttpContext,
                token,
                out var profileId,
                out var blobId))
            {
                return new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Error",
                            action = "Index",
                            id = "403"
                        }));
            }

            return new RedirectResult(
                BlobUrlCreator.CreateProfileBlobUrl(
                    profileId,
                    blobId));
        }

        [HttpGet]
        public ActionResult DownloadMessageBlob(
            [Bind(Prefix = DownloadActionQueryStringKey)] string token)
        {
            if (!BlobUrlCreator.TryParseMessageBlobWebPortalToken(
                this.HttpContext,
                token,
                out var profileId,
                out var messageId,
                out var blobId))
            {
                return new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Error",
                            action = "Index",
                            id = "403"
                        }));
            }

            return new RedirectResult(
                BlobUrlCreator.CreateMessageBlobUrl(
                    profileId,
                    messageId,
                    blobId));
        }
    }
}
