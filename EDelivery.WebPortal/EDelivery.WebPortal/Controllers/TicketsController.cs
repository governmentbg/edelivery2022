using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using ED.DomainServices.Tickets;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Grpc;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.Tickets;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Attributes;
using EDelivery.WebPortal.Utils.Filters;

using EDeliveryResources;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class TicketsController : Controller
    {
        private const int SystemTemplateId = 1;
        private const int SystemForwardTemplateId = 2;
        private const int TicketTemplate = 10001;

        private const int MaxTranslationsPerMessage = 5;

        private readonly Lazy<Ticket.TicketClient> ticketClient;
        private readonly Lazy<CachedUserData> userData;

        public TicketsController()
        {
            this.ticketClient =
                new Lazy<Ticket.TicketClient>(
                    () => GrpcClientFactory.CreateTicketClient(), isThreadSafe: false);

            this.userData =
                new Lazy<CachedUserData>(
                    () => this.HttpContext.GetCachedUserData(), isThreadSafe: false);
        }

        private CachedUserData UserData => this.userData.Value;

        [HttpGet]
        public ActionResult Distribute(string q)
        {
            try
            {
                string decoded = Utils.Utils.FromUrlSafeBase64(q);
                System.Collections.Specialized.NameValueCollection parameters =
                    HttpUtility.ParseQueryString(decoded);

                int profileId = int.Parse(parameters["p"]);
                int ticketId = int.Parse(parameters["t"]);

                if (!this.UserData.Profiles.Any(e => e.ProfileId == profileId))
                {
                    return RedirectToAction("Index", "Error", new { id = "403" });
                }

                int activeProfileId = this.UserData.ActiveProfileId;

                if (profileId != activeProfileId)
                {
                    this.UserData.ActiveProfileId = profileId;
                    this.UserData.BreadCrumb.ProfileName =
                        this.UserData.ActiveProfile.ProfileName;
                }

                return RedirectToAction("Open", "Tickets", new { id = ticketId });
            }
            catch
            {
                return RedirectToAction("Index", "Error", new { id = "403" });
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Inbox");
        }

        [StripAuthCookie]
        [OutputCache(NoStore = true, Duration = 0)]
        [HttpPost]
        public async Task<JsonResult> GetProfilesTicketsCounts()
        {
            try
            {
                GetNewTicketsCountResponse response =
                    await this.ticketClient.Value.GetNewTicketsCountAsync(
                        new GetNewTicketsCountRequest
                        {
                            LoginId = this.UserData.LoginId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                return Json(
                    new
                    {
                        Success = true,
                        Profiles = this.UserData
                            .Profiles
                            .Select(x =>
                                new
                                {
                                    IsCurrentProfile = x.ProfileId == UserData.ActiveProfileId,
                                    x.ProfileId,
                                    TicketsCount = response.NewTicketsCount.FirstOrDefault(e => e.ProfileId == x.ProfileId)?.Count
                                        ?? 0
                                })
                            .ToList()
                    });
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Error getting profiles messages counts.");
            }

            return Json(new { Success = false });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpGet]
        [BreadCrumb(2, typeof(Common), "TitleReceivedTickets", eLeftMenu.ReceivedTickets)]
        public async Task<ActionResult> Inbox(
            string from,
            string to,
            int page = 1)
        {
            DateTime? fromDT = DateTime.TryParseExact(
                from,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDT = DateTime.TryParseExact(
                to,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            InboxResponse response =
                await this.ticketClient.Value.InboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        From = fromDT?.ToTimestamp(),
                        To = toDT?.ToTimestamp(),
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            TicketsInboxViewModel model =
                new TicketsInboxViewModel()
                {
                    Tickets = new PagedList.PagedListLight<TicketsInboxViewModelItem>(
                        response.Result.Select(e => new TicketsInboxViewModelItem(e)).ToList(),
                        SystemConstants.PageSize,
                        page,
                        response.Length),
                    SearchFilter = new TicketsSearchViewModel(from, to)
                };

            if (model.Tickets.Count == 0)
            {
                ViewBag.NoMessages = model.SearchFilter.HasFilter
                    ? ProfilePage.LabelNoMessagesFromSearch
                    : ProfilePage.LabelNoReceivedMessages;
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public ActionResult Inbox(TicketsSearchViewModel model)
        {
            return RedirectToAction(
                nameof(TicketsController.Inbox),
                new
                {
                    from = model.From,
                    to = model.To,
                    page = 1,
                });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public async Task<ActionResult> ExportInbox(TicketsSearchViewModel model)
        {
            DateTime? fromDT = DateTime.TryParseExact(
                model.From,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt1)
                ? dt1
                : (DateTime?)null;

            DateTime? toDT = DateTime.TryParseExact(
                model.To,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dt2)
                ? dt2
                : (DateTime?)null;

            InboxResponse response =
                await this.ticketClient.Value.InboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                        From = fromDT?.ToTimestamp(),
                        To = toDT?.ToTimestamp(),
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return ExportService.ExportTicketsInbox(response);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        [BreadCrumb(3, typeof(Common), "TitleOpenTicket", eLeftMenu.ReceivedTickets)]
        public async Task<ActionResult> Open(
            [Bind(Prefix = "id")] int messageId)
        {
            await this.ticketClient.Value.OpenAsync(
                new OpenRequest
                {
                    LoginId = UserData.LoginId,
                    MessageId = messageId,
                    ProfileId = UserData.ActiveProfileId,
                },
                cancellationToken: Response.ClientDisconnectedToken);

            ReadResponse response =
                await this.ticketClient.Value.ReadAsync(
                    new ReadRequest
                    {
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (response.Message == null)
            {
                throw new HttpException(404, "Resource not found");
            }

            TicketsReadViewModel vm = new TicketsReadViewModel(response.Message);

            return View(vm);
        }

        [HttpGet]
        public ActionResult LoadObligations()
        {
            return View();
        }

        [ChildActionOnlyOrAjax]
        [HttpPost]
        public async Task<JsonResult> LoadObligationsInternal()
        {
            LoadMultipleObligationsResponse resp =
                await this.ticketClient.Value.LoadMultipleObligationsAsync(
                    new LoadMultipleObligationsRequest
                    {
                        ProfileId = UserData.ActiveProfileId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return Json(new
            {
                count = resp.Count,
                notFoundMessage = resp.NotFoundMessage ?? ErrorMessages.ErrorObligationNotFound,
            });
        }
    }
}
