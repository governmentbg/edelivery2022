using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using ED.DomainServices.Messages;
using ED.DomainServices.Translations;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Extensions;
using EDelivery.WebPortal.Grpc;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.Messages;
using EDelivery.WebPortal.Models.Templates;
using EDelivery.WebPortal.SeosService;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Attributes;
using EDelivery.WebPortal.Utils.Filters;

using EDeliveryResources;

using Grpc.Core;

using Newtonsoft.Json;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class MessagesController : Controller
    {
        private const int SystemTemplateId = 1;
        private const int SystemForwardTemplateId = 2;

        private const int MaxTranslationsPerMessage = 5;

        private readonly Lazy<Message.MessageClient> messageClient;
        private readonly Lazy<Translation.TranslationClient> translationClient;
        private readonly Lazy<CachedUserData> userData;

        public MessagesController()
        {
            this.messageClient =
                new Lazy<Message.MessageClient>(
                    () => GrpcClientFactory.CreateMessageClient(), isThreadSafe: false);

            this.translationClient =
                new Lazy<Translation.TranslationClient>(
                    () => GrpcClientFactory.CreateTranslationClient(), isThreadSafe: false);

            this.userData =
                new Lazy<CachedUserData>(
                    () => this.HttpContext.GetCachedUserData(), isThreadSafe: false);
        }

        private CachedUserData UserData => this.userData.Value;

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("Inbox");
        }

        //TODO: How to handle cancellation exception here?
        [StripAuthCookie] // this method should not renew identity cookie expiration
        [OutputCache(NoStore = true, Duration = 0)]
        [HttpPost]
        public async Task<JsonResult> GetProfilesMessagesCounts()
        {
            try
            {
                GetNewMessagesCountResponse response =
                    await this.messageClient.Value.GetNewMessagesCountAsync(
                        new GetNewMessagesCountRequest
                        {
                            LoginId = this.UserData.LoginId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                Dictionary<int, int> seosNewMessagesCount =
                    this.GetSEOSNewMessagesCount();

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
                                    NewMessages = response.NewMessagesCount.FirstOrDefault(e => e.ProfileId == x.ProfileId)?.Count
                                        ?? 0,
                                    NewSEOSMessages = seosNewMessagesCount.ContainsKey(x.ProfileId)
                                        ? seosNewMessagesCount[x.ProfileId]
                                        : 0
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
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleReceivedMessages", eLeftMenu.ReceivedMessages)]
        public async Task<ActionResult> Inbox(
            string subject,
            string profile,
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
                await this.messageClient.Value.InboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        Subject = subject,
                        Profile = profile,
                        From = fromDT?.ToTimestamp(),
                        To = toDT?.ToTimestamp(),
                        Rnu = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            InboxViewModel model =
                new InboxViewModel(this.UserData.ActiveProfile.TargetGroupId)
                {
                    Messages = new PagedList.PagedListLight<InboxResponse.Types.Message>(
                        response.Result.ToList(),
                        SystemConstants.PageSize,
                        page,
                        response.Length),
                    SearchFilter = new SearchMessagesViewModel(
                        subject,
                        profile,
                        from,
                        to,
                        BoxType.Inbox)
                };

            if (model.Messages.Count == 0)
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
        public ActionResult Inbox(SearchMessagesViewModel model)
        {
            return RedirectToAction("Inbox", new
            {
                subject = model.Subject,
                profile = model.Profile,
                from = model.From,
                to = model.To,
                page = 1,
            });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public async Task<ActionResult> ExportInbox(SearchMessagesViewModel model)
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
                await this.messageClient.Value.InboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                        Subject = model.Subject,
                        Profile = model.Profile,
                        From = fromDT?.ToTimestamp(),
                        To = toDT?.ToTimestamp(),
                        Rnu = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return ExportService.ExportInbox(response);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleSentMessages", eLeftMenu.SentMessages)]
        public async Task<ActionResult> Outbox(
            string subject,
            string profile,
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

            OutboxResponse response =
                await this.messageClient.Value.OutboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        Subject = subject,
                        Profile = profile,
                        From = fromDT?.ToTimestamp(),
                        To = toDT?.ToTimestamp(),
                        Rnu = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            OutboxViewModel model =
                new OutboxViewModel(this.UserData.ActiveProfile.TargetGroupId)
                {
                    Messages = new PagedList.PagedListLight<OutboxResponse.Types.Message>(
                        response.Result.ToList(),
                        SystemConstants.PageSize,
                        page,
                        response.Length),
                    SearchFilter = new SearchMessagesViewModel(
                        subject,
                        profile,
                        from,
                        to,
                        BoxType.Outbox)
                };

            if (model.Messages.Count == 0)
            {
                ViewBag.NoMessages = model.SearchFilter.HasFilter
                    ? ProfilePage.LabelNoMessagesFromSearch
                    : ProfilePage.LabelNoSentMessages;
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public ActionResult Outbox(SearchMessagesViewModel model)
        {
            return RedirectToAction("Outbox", new
            {
                subject = model.Subject,
                profile = model.Profile,
                from = model.From,
                to = model.To,
                page = 1,
            });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public async Task<ActionResult> ExportOutbox(SearchMessagesViewModel model)
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

            OutboxResponse response =
                await this.messageClient.Value.OutboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Offset = 0,
                        Limit = SystemConstants.ExportSize,
                        Subject = model.Subject,
                        Profile = model.Profile,
                        From = fromDT?.ToTimestamp(),
                        To = toDT?.ToTimestamp(),
                        Rnu = null,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return ExportService.ExportOutbox(response);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSenderOrRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<FileResult> GetMessageTimestamp(
            [Bind(Prefix = "id")] int messageId,
            eTimeStampType timeStampType)
        {
            GetTimestampResponse response;

            switch (timeStampType)
            {
                case eTimeStampType.NRO:
                    response = await this.messageClient.Value.GetTimestampNROAsync(
                        new GetTimestampRequest
                        {
                            MessageId = messageId,
                            ProfileId = UserData.ActiveProfileId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);
                    break;
                case eTimeStampType.NRD:
                    response = await this.messageClient.Value.GetTimestampNRDAsync(
                        new GetTimestampRequest
                        {
                            MessageId = messageId,
                            ProfileId = UserData.ActiveProfileId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);
                    break;
                case eTimeStampType.Document:
                default:
                    throw new NotImplementedException();
            }

            return File(
                response.Timestamp.ToArray(),
                "application/octet-stream",
                response.FileName);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSenderOrRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<FileResult> GetDocumentTimestamp(
            [Bind(Prefix = "id")] int messageId,
            int documentId)
        {
            GetBlobTimestampResponse response =
                await this.messageClient.Value.GetBlobTimestampAsync(
                    new GetBlobTimestampRequest
                    {
                        BlobId = documentId,
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Timestamp.ToArray(),
                "application/octet-stream",
                response.FileName);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<FileResult> GetSummaryAsRecipient(
            [Bind(Prefix = "id")] int messageId)
        {
            GetSummaryAsRecipientResponse response =
                await this.messageClient.Value.GetSummaryAsRecipientAsync(
                    new GetSummaryAsRecipientRequest
                    {
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Summary.ToArray(),
                response.ContentType,
                response.FileName);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSender,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<FileResult> GetSummaryAsSender(
            [Bind(Prefix = "id")] int messageId)
        {
            GetSummaryAsSenderResponse response =
                await this.messageClient.Value.GetSummaryAsSenderAsync(
                    new GetSummaryAsSenderRequest
                    {
                        MessageId = messageId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Summary.ToArray(),
                response.ContentType,
                response.FileName);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSender,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<FileResult> GetPdfAsSender(
            [Bind(Prefix = "id")] int messageId)
        {
            GetPdfAsSenderResponse response =
                await this.messageClient.Value.GetPdfAsSenderAsync(
                new GetPdfAsSenderRequest
                {
                    MessageId = messageId,
                },
                cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Content.ToArray(),
                response.ContentType,
                response.FileName);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<FileResult> GetPdfAsRecipient(
            [Bind(Prefix = "id")] int messageId)
        {
            GetPdfAsRecipientResponse response =
               await this.messageClient.Value.GetPdfAsRecipientAsync(
               new GetPdfAsRecipientRequest
               {
                   MessageId = messageId,
                   ProfileId = this.UserData.ActiveProfileId,
               },
               cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Content.ToArray(),
                response.ContentType,
                response.FileName);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.MessageAccess,
            MessageIdRouteOrQueryParam = "messageId")]
        [HttpGet]
        public async Task<ActionResult> MessageProfileInfo(int messageId)
        {
            GetSenderProfileResponse profile =
                await this.messageClient.Value.GetSenderProfileAsync(
                    new GetSenderProfileRequest
                    {
                        MessageId = messageId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            MessageProfileInfoViewModel vm =
                new MessageProfileInfoViewModel(profile);

            return PartialView("Partials/_MessageProfileInfo", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        [BreadCrumb(3, typeof(EDeliveryResources.Common), "TitleOpenMessage", eLeftMenu.ReceivedMessages)]
        public async Task<ActionResult> Open(
            [Bind(Prefix = "id")] int messageId)
        {
            await this.messageClient.Value.OpenAsync(
                new OpenRequest
                {
                    LoginId = UserData.LoginId,
                    MessageId = messageId,
                    ProfileId = UserData.ActiveProfileId
                },
                cancellationToken: Response.ClientDisconnectedToken);

            ReadResponse response =
                await this.messageClient.Value.ReadAsync(
                    new ReadRequest
                    {
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (response.Message == null)
            {
                throw new HttpException(404, "Resource not found");
            }

            ReadMessageViewModel vm = new ReadMessageViewModel(
                response.Message,
                response.ForwardedMessage);

            GetTemplateContentResponse template =
                await this.messageClient.Value.GetTemplateContentAsync(
                    new GetTemplateContentRequest
                    {
                        TemplateId = vm.TemplateId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            GetTemplateContentResponse forwardedTemplate = null;

            if (vm.ForwardedMessage != null)
            {
                forwardedTemplate =
                    await this.messageClient.Value.GetTemplateContentAsync(
                        new GetTemplateContentRequest
                        {
                            TemplateId = vm.ForwardedMessage.TemplateId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);
            }

            vm.SetFields(
                template.Content,
                forwardedTemplate?.Content);

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSender,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        [BreadCrumb(3, typeof(EDeliveryResources.Common), "TitleOpenSentMessage", eLeftMenu.SentMessages)]
        public async Task<ActionResult> View(
            [Bind(Prefix = "id")] int messageId)
        {
            ViewResponse response =
                await this.messageClient.Value.ViewAsync(
                    new ViewRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        MessageId = messageId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (response.Message == null)
            {
                throw new HttpException(404, "Resource not found");
            }

            ViewMessageViewModel vm = new ViewMessageViewModel(
                response.Message,
                response.ForwardedMessage);

            GetTemplateContentResponse template =
                await this.messageClient.Value.GetTemplateContentAsync(
                    new GetTemplateContentRequest
                    {
                        TemplateId = vm.TemplateId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            GetTemplateContentResponse forwardedTemplate = null;

            if (vm.ForwardedMessage != null)
            {
                forwardedTemplate =
                    await this.messageClient.Value.GetTemplateContentAsync(
                        new GetTemplateContentRequest
                        {
                            TemplateId = vm.ForwardedMessage.TemplateId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);
            }

            vm.SetFields(
                template.Content,
                forwardedTemplate?.Content);

            return View(vm);
        }

        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessage", eLeftMenu.CreateMessage)]
        public async Task<ActionResult> ChooseReply(
            [Bind(Prefix = "id")] int messageId,
            int? forwardingMessageId)
        {
            GetReplyResponse reply =
                await this.messageClient.Value.GetReplyAsync(
                    new GetReplyRequest
                    {
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (reply.ResponseTemplateId.HasValue)
            {
                return RedirectToAction(
                    "Reply",
                    new
                    {
                        id = messageId,
                        templateId = reply.ResponseTemplateId.Value,
                        forwardingMessageId,
                    });
            }

            GetAllowedTemplatesResponse templates =
                await this.messageClient.Value.GetAllowedTemplatesAsync(
                    new GetAllowedTemplatesRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            List<MessageTemplateInfoViewModel> templatesVM = templates
                .Result
                .Select(e =>
                    new MessageTemplateInfoViewModel
                    {
                        TemplateId = e.TemplateId,
                        Name = e.Name,
                    })
                .ToList();

            if (templatesVM.Count() == 0)
            {
                return RedirectToAction(
                    "Index",
                    "Error",
                    new { id = "403" });
            }

            if (templatesVM.Count() == 1)
            {
                int replyTemplateId = templatesVM.First().TemplateId;

                return RedirectToAction(
                    "Reply",
                    new
                    {
                        id = messageId,
                        templateId = replyTemplateId,
                        forwardingMessageId,
                    });
            }

            ChooseReplyViewModel vm = new ChooseReplyViewModel
            {
                MessageId = messageId,
                Templates = templatesVM,
                ForwardingMessageId = forwardingMessageId,
            };

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReplyMessage,
             MessageIdRouteOrQueryParam = "id",
            TemplateIdRouteOrQueryParam = "templateId",
            ForwardingMessageIdRouteOrQueryParam = "forwardingMessageId")]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessage", eLeftMenu.CreateMessage)]
        public async Task<ActionResult> Reply(
            [Bind(Prefix = "id")] int messageId,
            int templateId,
            int? forwardingMessageId)
        {
            GetReplyResponse reply =
                await this.messageClient.Value.GetReplyAsync(
                    new GetReplyRequest
                    {
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            NewMessageViewModel newMessageVM =
                new NewMessageViewModel
                {
                    CurrentProfileId = UserData.ActiveProfileId,
                    RecipientIds = $"p{reply.RecipientProfileId}",
                    RecipientNames = reply.RecipientName,
                    TemplateId = templateId,
                    Subject = $"RE: {reply.Subject}",
                    Rnu = reply.Rnu,
                };

            this.SetTempModel(newMessageVM, false);

            return RedirectToAction(
                "New",
                new { templateId });
        }

        // TODO: consider changing the action name
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessage", eLeftMenu.CreateMessage)]
        public async Task<ActionResult> Template()
        {
            GetAllowedTemplatesResponse templates =
                await this.messageClient.Value.GetAllowedTemplatesAsync(
                    new GetAllowedTemplatesRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            IEnumerable<MessageTemplateInfoViewModel> vm = templates
                .Result
                .Select(e =>
                    new MessageTemplateInfoViewModel
                    {
                        TemplateId = e.TemplateId,
                        Name = e.Name
                    })
                .ToList();

            if (vm.Count() == 0)
            {
                return RedirectToAction(
                    "Index",
                    "Error",
                    new { id = "403" });
            }

            if (vm.Count() == 1)
            {
                int templateId = vm.First().TemplateId;

                return RedirectToAction("New", new { templateId });
            }

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.WriteMessage,
            TemplateIdRouteOrQueryParam = "templateId")]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessage", eLeftMenu.CreateMessage)]
        public async Task<ActionResult> New(int templateId)
        {
            ExistsTemplateResponse existsTemplateResponse =
                await this.messageClient.Value.ExistsTemplateAsync(
                    new ExistsTemplateRequest
                    {
                        TemplateId = templateId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            if (!existsTemplateResponse.Exists)
            {
                throw new ArgumentException("Invalid template.");
            }

            NewMessageViewModel model = new NewMessageViewModel
            {
                CurrentProfileId = UserData.ActiveProfileId,
                TemplateId = templateId,
            };

            NewMessageViewModel postModel =
                this.GetTempModel<NewMessageViewModel>(true);

            if (postModel != null)
            {
                model = postModel;
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.WriteMessage,
            TemplateIdRouteOrQueryParam = "templateId")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New(
            int templateId,
            NewMessageViewModel model,
            FormCollection form)
        {
            try
            {
                GetTemplateContentResponse templateContent =
                    await this.messageClient.Value.GetTemplateContentAsync(
                        new GetTemplateContentRequest
                        {
                            TemplateId = model.TemplateId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                Dictionary<string, string[]> formValues =
                    TemplatesService.ToDictionary(form);

                Dictionary<string, string> templateValidationErrors =
                    TemplatesService.Validate(form, templateContent.Content);

                model.TemplateValuesAsJson =
                    JsonConvert.SerializeObject(formValues);

                if (templateValidationErrors.Any())
                {
                    List<BuilderModelStateError> errors = new List<BuilderModelStateError>();

                    foreach (KeyValuePair<string, string> error in templateValidationErrors)
                    {
                        errors.Add(new BuilderModelStateError
                        {
                            Key = error.Key,
                            Message = error.Value
                        });

                        ModelState.AddModelError(error.Key.ToString(), error.Value);
                    }

                    model.TemplateErrorsAsJson =
                        JsonConvert.SerializeObject(errors);
                }

                if (!ModelState.IsValid)
                {
                    this.SetTempModel(model, true);

                    return RedirectToAction("New", new { templateId });
                }

                (string jsonMetaFields, string jsonPayload, int[] blobIds) =
                    TemplatesService.ExtractPayloads(templateContent.Content, form);

                int[] recipientProfileIds = model.RecipientIds
                    .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(e => e.StartsWith("p"))
                    .Select(e => e.Substring(1))
                    .Select(e => int.Parse(e))
                    .ToArray();

                int[] recipientGroupIds = model.RecipientIds
                    .Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(e => e.StartsWith("g"))
                    .Select(e => e.Substring(1))
                    .Select(e => int.Parse(e))
                    .ToArray();

                SendRequest request =
                    new SendRequest
                    {
                        BlobIds = { blobIds },
                        Body = jsonPayload,
                        MetaFields = jsonMetaFields,
                        RecipientGroupIds =
                        {
                            recipientGroupIds
                        },
                        RecipientProfileIds =
                        {
                            recipientProfileIds
                        },
                        SenderProfileId = this.UserData.ActiveProfileId,
                        SenderLoginId = this.UserData.LoginId,
                        Subject = model.Subject,
                        TemplateId = model.TemplateId,
                        ForwardedMessageId = null,
                        Rnu = !string.IsNullOrEmpty(model.Rnu)
                            ? model.Rnu
                            : null,
                    };

                _ = await this.messageClient.Value.SendAsync(
                    request,
                    cancellationToken: Response.ClientDisconnectedToken);

                return RedirectToAction("Outbox");
            }
            catch (RpcException er)
            {
                ElmahLogger.Instance.Error(er, er.Message);

                ModelState.AddModelError(string.Empty, ErrorMessages.ErrorCantSendDocument);

                this.SetTempModel(model, true);

                return RedirectToAction("New", new { templateId });
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Can not send message from login {User.Identity.Name} to profile Ids {model.RecipientIds}!");

                ModelState.AddModelError(string.Empty, ErrorMessages.ErrorCantSendDocument);

                this.SetTempModel(model, true);

                return RedirectToAction("New", new { templateId });
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ForwardMessage,
            MessageIdRouteOrQueryParam = "messageId")]
        [HttpGet]
        public async Task<ActionResult> Forward(int messageId)
        {
            GetForwardMessageInfoResponse info =
                await this.messageClient.Value.GetForwardMessageInfoAsync(
                    new GetForwardMessageInfoRequest
                    {
                        MessageId = messageId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            ForwardMessageModel model =
                new ForwardMessageModel(messageId, info)
                {
                    ForwardTemplateId = SystemForwardTemplateId,
                    Rnu = info.Rnu,
                };

            return PartialView("Modals/_Forward", model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ForwardMessage,
            MessageIdFormValueParam = "ForwardMessageId")]
        [HttpPost]
        public async Task<ActionResult> Forward(
            ForwardMessageModel model,
            FormCollection form)
        {
            try
            {
                GetTemplateContentResponse templateContent =
                await this.messageClient.Value.GetTemplateContentAsync(
                    new GetTemplateContentRequest
                    {
                        TemplateId = model.ForwardTemplateId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                Dictionary<string, string[]> formValues =
                    TemplatesService.ToDictionary(form);

                Dictionary<string, string> templateValidationErrors =
                    TemplatesService.Validate(form, templateContent.Content);

                if (templateValidationErrors.Any())
                {
                    List<BuilderModelStateError> errors = new List<BuilderModelStateError>();

                    foreach (KeyValuePair<string, string> error in templateValidationErrors)
                    {
                        errors.Add(new BuilderModelStateError
                        {
                            Key = error.Key,
                            Message = error.Value
                        });

                        ModelState.AddModelError(error.Key.ToString(), error.Value);
                    }

                    model.TemplateValuesAsJson =
                        JsonConvert.SerializeObject(formValues);
                    model.TemplateErrorsAsJson =
                        JsonConvert.SerializeObject(errors);
                }

                if (!ModelState.IsValid)
                {
                    return PartialView("Modals/_Forward", model);
                }

                (string jsonMetaFields, string jsonPayload, _) =
                    TemplatesService.ExtractPayloads(templateContent.Content, form);

                SendRequest request =
                    new SendRequest
                    {
                        Body = jsonPayload,
                        MetaFields = jsonMetaFields,
                        RecipientGroupIds =
                        {
                            Array.Empty<int>()
                        },
                        RecipientProfileIds =
                        {
                            new int[] { model.ForwardRecipientProfileId }
                        },
                        SenderProfileId = this.UserData.ActiveProfileId,
                        SenderLoginId = this.UserData.LoginId,
                        Subject = model.ForwardSubject,
                        TemplateId = model.ForwardTemplateId,
                        ForwardedMessageId = model.ForwardMessageId,
                        Rnu = model.Rnu,
                    };

                _ = await this.messageClient.Value.SendAsync(
                    request,
                    cancellationToken: Response.ClientDisconnectedToken);

                return PartialView("Modals/_ForwardMessageSuccess");
            }
            catch (RpcException er)
            {
                ElmahLogger.Instance.Error(er, er.Message);

                ModelState.AddModelError(
                    "ForwardRecipientProfileId",
                    er.Message);

                return PartialView("Modals/_Forward", model);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Can not forward message from login {User.Identity.Name} to profile Id {model.ForwardRecipientProfileId}!");

                ModelState.AddModelError(
                    "ForwardRecipientProfileId",
                    ErrorMessages.ErrorCantSendDocument);

                return PartialView("Modals/_Forward", model);
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSenderOrRecipient,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<ActionResult> GetForwardedMessageHistory(
            [Bind(Prefix = "id")] int messageId)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Outbox");
            }

            GetForwardHistoryResponse response =
                await this.messageClient.Value.GetForwardHistoryAsync(
                    new GetForwardHistoryRequest
                    {
                        MessageId = messageId,
                        ProfileId = UserData.ActiveProfileId
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            List<ForwardHistoryViewModel> vm = response
                .History
                .Select(e => new ForwardHistoryViewModel(e))
                .ToList();

            if (vm.Any())
            {
                return PartialView("Partials/_ForwardedMessageHistory", vm);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.ReadMessageAsSenderOrRecipient, // TODO: check if recipient should be able to access this
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        public async Task<ActionResult> GetMessageRecipients(
            [Bind(Prefix = "id")] int messageId)
        {
            if (!Request.IsAjaxRequest())
            {
                return RedirectToAction("Outbox");
            }

            GetMessageRecipientsResponse response =
                await this.messageClient.Value.GetMessageRecipientsAsync(
                    new GetMessageRecipientsRequest
                    {
                        MessageId = messageId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            List<MessageRecipientsViewModel> vm = response
                .Recipients
                .Select(e => new MessageRecipientsViewModel(e))
                .ToList();

            if (vm.Any())
            {
                return PartialView("Partials/_MessageRecipients", vm);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.MessageAccess,
            MessageIdRouteOrQueryParam = "messageId")]
        [HttpGet]
        public ActionResult CreateTranslation(int messageId)
        {
            List<SelectListItem> Languages = new List<SelectListItem>
            {
                new SelectListItem() { Value = "", Text = "", Selected = true },
                new SelectListItem() { Value = "BG", Text = "Български (BG)" },
                new SelectListItem() { Value = "EN", Text = "English (EN)" },
                new SelectListItem() { Value = "DE", Text = "German (DE)" },
                new SelectListItem() { Value = "FR", Text = "French (FR)" },
                new SelectListItem() { Value = "ES", Text = "Spanish (ES)" },
                new SelectListItem() { Value = "IT", Text = "Italian (IT)" },
                new SelectListItem() { Value = "RO", Text = "Romanian (RO)" },
                new SelectListItem() { Value = "HR", Text = "Croatian (HR)" },
                new SelectListItem() { Value = "FI", Text = "Finnish (FI)" },
                new SelectListItem() { Value = "LV", Text = "Latvian (LV)" },
                new SelectListItem() { Value = "RU", Text = "Russian (RU)" },
                new SelectListItem() { Value = "CS", Text = "Czech (CS)" },
                new SelectListItem() { Value = "LT", Text = "Lithuanian (LT)" },
                new SelectListItem() { Value = "DA", Text = "Danish (DA)" },
                new SelectListItem() { Value = "EL", Text = "Greek (EL)" },
                new SelectListItem() { Value = "MT", Text = "Maltese (MT)" },
                new SelectListItem() { Value = "SK", Text = "Slovak (SK)" },
                new SelectListItem() { Value = "NL", Text = "Dutch (NL)" },
                new SelectListItem() { Value = "HU", Text = "Hungarian (HU)" },
                new SelectListItem() { Value = "NB", Text = "Norwegian Bokmål (NB)" },
                new SelectListItem() { Value = "SL", Text = "Slovenian (SL)" },
                new SelectListItem() { Value = "IS", Text = "Icelandic (IS)" },
                new SelectListItem() { Value = "PL", Text = "Polish (PL)" },
                new SelectListItem() { Value = "SV", Text = "Swedish (SV)" },
                new SelectListItem() { Value = "ET", Text = "Estonian (ET)" },
                new SelectListItem() { Value = "GA", Text = "Irish (GA)" },
                new SelectListItem() { Value = "PT", Text = "Portuguese (PT)" },
            };

            ViewBag.Languages = Languages;

            CreateTranslationViewModel vm =
                this.GetTempModel<CreateTranslationViewModel>(true)
                    ?? new CreateTranslationViewModel(messageId);

            return PartialView("Modals/_CreateTranslation", vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.MessageAccess,
            MessageIdFormValueParam = "MessageId")]
        [HttpPost]
        public async Task<ActionResult> CreateTranslation(
            CreateTranslationViewModel model)
        {
            try
            {
                if (!string.IsNullOrEmpty(model.SourceLanguage)
                    && model.SourceLanguage == model.TargetLanguage)
                {
                    ModelState.AddModelError(
                        nameof(CreateTranslationViewModel.TargetLanguage),
                        "Целевият език трябва да е различен от оригиналния.");
                }

                if (!string.IsNullOrEmpty(model.SourceLanguage)
                    && !string.IsNullOrEmpty(model.TargetLanguage))
                {
                    CheckExistingMessageTranslationResponse resp =
                        await this.translationClient.Value.CheckExistingMessageTranslationAsync(
                            new CheckExistingMessageTranslationRequest
                            {
                                MessageId = model.MessageId,
                                ProfileId = this.UserData.ActiveProfileId,
                                SourceLanguage = model.SourceLanguage,
                                TargetLanguage = model.TargetLanguage,
                            },
                            cancellationToken: Response.ClientDisconnectedToken);

                    if (resp.IsExisting)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Вече е създадена заявка със същите параметри.");
                    }

                    GetMessageTranslationsCountResponse resp2 =
                        await this.translationClient.Value.GetMessageTranslationsCountAsync(
                            new GetMessageTranslationsCountRequest
                            {
                                MessageId = model.MessageId,
                                ProfileId = this.UserData.ActiveProfileId,
                            },
                            cancellationToken: Response.ClientDisconnectedToken);

                    if (resp2.Count >= MaxTranslationsPerMessage)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Стигнали сте максималния брой на преводи за съобщение.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    this.SetTempModel(model, true);

                    return RedirectToAction(
                        nameof(MessagesController.CreateTranslation),
                        new { messageId = model.MessageId });
                }

                _ = await this.translationClient.Value.AddMessageTranslationAsync(
                    new AddMessageTranslationRequest
                    {
                        MessageId = model.MessageId,
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        SourceLanguage = model.SourceLanguage,
                        TargetLanguage = model.TargetLanguage,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

                return PartialView("Modals/_CreateTranslationSuccess");
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Can not create message [{model.MessageId}] translation!");

                ModelState.AddModelError(string.Empty, ErrorMessages.ErrorCantCreateTranslation);

                this.SetTempModel(model, true);

                return RedirectToAction(
                        nameof(MessagesController.CreateTranslation),
                        new { messageId = model.MessageId });
            }
        }

        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessage", eLeftMenu.CreateMessageByTemplateCategory)]
        public async Task<ActionResult> TemplatesByCategory(string category)
        {
            throw new NotImplementedException();

#pragma warning disable CS0162 // Unreachable code detected
            GetTemplatesByCategoryResponse templates =
                await this.messageClient.Value.GetTemplatesByCategoryAsync(
                    new GetTemplatesByCategoryRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Category = category,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            IEnumerable<MessageTemplateInfoViewModel> vm = templates
                .Result
                .Select(e =>
                    new MessageTemplateInfoViewModel
                    {
                        TemplateId = e.TemplateId,
                        Name = e.Name
                    })
                .ToList();

            if (vm.Count() == 0)
            {
                return RedirectToAction(
                    "Index",
                    "Error",
                    new { id = "403" });
            }

            return View(vm);
#pragma warning restore CS0162 // Unreachable code detected
        }

        #region Old urls

        [HttpGet]
        public ActionResult ViewMessage([Bind(Prefix = "id")] int messageId)
        {
            return RedirectToAction("View", new { id = messageId });
        }

        [HttpGet]
        public ActionResult OpenReceivedMessage(
            [Bind(Prefix = "id")] int messageId)
        {
            return RedirectToAction("Open", new { id = messageId });
        }

        [HttpGet]
        public ActionResult ReceivedMessages(int page = 1)
        {
            return RedirectToAction("Inbox", new { page });
        }

        [HttpGet]
        public ActionResult SentMessages(int page = 1)
        {
            return RedirectToAction("Outbox", new { page });
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult OpenWithCode()
        {
            return RedirectToAction("Index", "CodeMessages");
        }

        public ActionResult CreateMessage()
        {
            return RedirectToAction(
                "New",
                "Messages",
                new { templateId = SystemTemplateId });
        }

        #endregion

        #region Private methods

        private Dictionary<int, int> GetSEOSNewMessagesCount()
        {
            if (this.UserData.Profiles.Any(e => e.HasSEOS ?? false))
            {
                try
                {
                    using (SEOSPostServiceClient seosClient = new SEOSPostServiceClient())
                    {
                        Dictionary<Guid, int> profilesMsgCount =
                            seosClient.GetNewMessagesCount(
                                this.UserData
                                    .Profiles
                                    .Where(p => p.TargetGroupId == (int)TargetGroupId.PublicAdministration)
                                    .Select(p => p.ProfileGuid)
                                    .ToArray());

                        Dictionary<int, int> result = this.UserData
                            .Profiles
                            .Where(p => profilesMsgCount.ContainsKey(p.ProfileGuid))
                            .ToDictionary(
                                k => k.ProfileId,
                                v => profilesMsgCount[v.ProfileGuid]);

                        return result;
                    }
                }
                catch
                {
                    return new Dictionary<int, int>();
                }
            }

            return new Dictionary<int, int>();
        }

        #endregion Private methods
    }
}
