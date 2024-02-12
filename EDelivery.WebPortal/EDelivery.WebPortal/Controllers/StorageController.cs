using System;
using System.Collections.Generic;
using System.Globalization;
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

        [HttpPost]
        public async Task<ActionResult> ListFreeBlobs(
            string freeBlobsFileName,
            string freeBlobsAuthor,
            string freeBlobsFromDate,
            string freeBlobsToDate,
            int page = 1)
        {
            DateTime? fromDateDt = DateTime.TryParseExact(
                freeBlobsFromDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDateDt = DateTime.TryParseExact(
                freeBlobsToDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            SearchFreeBlobsViewModel model = new SearchFreeBlobsViewModel()
            {
                FreeBlobsAuthor = freeBlobsAuthor,
                FreeBlobsFileName = freeBlobsFileName,
                FreeBlobsFromDate = freeBlobsFromDate,
                FreeBlobsToDate = freeBlobsToDate,
            };

            GetProfileFreeBlobsResponse blobs =
                await this.blobClient.Value.GetProfileFreeBlobsAsync(
                    new GetProfileFreeBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        FileName = model.FreeBlobsFileName ?? string.Empty,
                        Author = model.FreeBlobsAuthor ?? string.Empty,
                        FromDate = fromDateDt?.ToTimestamp(),
                        ToDate = toDateDt?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ListFreeBlobsViewModel vm =
                new ListFreeBlobsViewModel(model, blobs, SystemConstants.PageSize, page);

            return PartialView("Partials/ListFreeBlobs", vm);
        }

        [HttpPost]
        public async Task<ActionResult> ExportFreeBlobs(
            SearchFreeBlobsViewModel model)
        {
            DateTime? fromDateDt = DateTime.TryParseExact(
                model.FreeBlobsFromDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDateDt = DateTime.TryParseExact(
                model.FreeBlobsToDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            GetProfileFreeBlobsResponse blobs =
                await this.blobClient.Value.GetProfileFreeBlobsAsync(
                    new GetProfileFreeBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                        FileName = model.FreeBlobsFileName ?? string.Empty,
                        Author = model.FreeBlobsAuthor ?? string.Empty,
                        FromDate = fromDateDt?.ToTimestamp(),
                        ToDate = toDateDt?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return ExportService.ExportFreeBlobs(blobs);
        }

        [HttpPost]
        public async Task<ActionResult> ListInboxBlobs(
            string inboxBlobsFileName,
            string inboxBlobsSubject,
            string inboxBlobsFromDate,
            string inboxBlobsToDate,
            int page = 1)
        {
            DateTime? fromDateDt = DateTime.TryParseExact(
                inboxBlobsFromDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDateDt = DateTime.TryParseExact(
                inboxBlobsToDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            SearchInboxBlobsViewModel model = new SearchInboxBlobsViewModel()
            {
                InboxBlobsFileName = inboxBlobsFileName,
                InboxBlobsMessageSubject = inboxBlobsSubject,
                InboxBlobsFromDate = inboxBlobsFromDate,
                InboxBlobsToDate = inboxBlobsToDate,
            };

            GetProfileInboxBlobsResponse blobs =
                await this.blobClient.Value.GetProfileInboxBlobsAsync(
                    new GetProfileInboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        FileName = model.InboxBlobsFileName ?? string.Empty,
                        MessageSubject = model.InboxBlobsMessageSubject ?? string.Empty,
                        FromDate = fromDateDt?.ToTimestamp(),
                        ToDate = toDateDt?.ToTimestamp()
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

        [HttpPost]
        public async Task<ActionResult> ExportInboxBlobs(
            SearchInboxBlobsViewModel model)
        {
            DateTime? fromDateDt = DateTime.TryParseExact(
                model.InboxBlobsFromDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDateDt = DateTime.TryParseExact(
                model.InboxBlobsToDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            GetProfileInboxBlobsResponse blobs =
                await this.blobClient.Value.GetProfileInboxBlobsAsync(
                    new GetProfileInboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                        FileName = model.InboxBlobsFileName ?? string.Empty,
                        MessageSubject = model.InboxBlobsMessageSubject ?? string.Empty,
                        FromDate = fromDateDt?.ToTimestamp(),
                        ToDate = toDateDt?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return ExportService.ExportInboxBlobs(blobs);
        }

        [HttpPost]
        public async Task<ActionResult> ListOutboxBlobs(
            string outboxBlobsFileName,
            string outboxBlobsSubject,
            string outboxBlobsFromDate,
            string outboxBlobsToDate,
            int page = 1)
        {
            DateTime? fromDateDt = DateTime.TryParseExact(
                outboxBlobsFromDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDateDt = DateTime.TryParseExact(
                outboxBlobsToDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            SearchOutboxBlobsViewModel model = new SearchOutboxBlobsViewModel()
            {
                OutboxBlobsFileName = outboxBlobsFileName,
                OutboxBlobsMessageSubject = outboxBlobsSubject,
                OutboxBlobsFromDate = outboxBlobsFromDate,
                OutboxBlobsToDate = outboxBlobsToDate,
            };

            GetProfileOutboxBlobsResponse blobs =
                await this.blobClient.Value.GetProfileOutboxBlobsAsync(
                    new GetProfileOutboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        FileName = model.OutboxBlobsFileName ?? string.Empty,
                        MessageSubject = model.OutboxBlobsMessageSubject ?? string.Empty,
                        FromDate = fromDateDt?.ToTimestamp(),
                        ToDate = toDateDt?.ToTimestamp()
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

        [HttpPost]
        public async Task<ActionResult> ExportOutboxBlobs(
            SearchOutboxBlobsViewModel model)
        {
            DateTime? fromDateDt = DateTime.TryParseExact(
                model.OutboxBlobsFromDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDateDt = DateTime.TryParseExact(
                model.OutboxBlobsToDate,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            GetProfileOutboxBlobsResponse blobs =
                await this.blobClient.Value.GetProfileOutboxBlobsAsync(
                    new GetProfileOutboxBlobsRequest
                    {
                        LoginId = UserData.LoginId,
                        ProfileId = UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                        FileName = model.OutboxBlobsFileName ?? string.Empty,
                        MessageSubject = model.OutboxBlobsMessageSubject ?? string.Empty,
                        FromDate = fromDateDt?.ToTimestamp(),
                        ToDate = toDateDt?.ToTimestamp()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return ExportService.ExportOutboxBlobs(blobs);
        }

        [HttpGet]
        public ActionResult DownloadProfileBlob(
            [Bind(Prefix = DownloadActionQueryStringKey)] string token)
        {
            if (!BlobUrlCreator.TryParseProfileBlobWebPortalToken(
                this.HttpContext,
                token,
                out int profileId,
                out int blobId))
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
                out int profileId,
                out int messageId,
                out int blobId))
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
