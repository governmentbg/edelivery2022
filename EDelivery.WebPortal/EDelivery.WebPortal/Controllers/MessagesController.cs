using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using ED.DomainServices.Messages;

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

        private readonly Lazy<Message.MessageClient> messageClient;
        private readonly Lazy<CachedUserData> userData;

        public MessagesController()
        {
            this.messageClient =
                new Lazy<Message.MessageClient>(
                    () => GrpcClientFactory.CreateMessageClient(), isThreadSafe: false);

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

        [StripAuthCookie]
        [OutputCache(NoStore = true, Duration = 0)]
        [HttpGet]
        public async Task<JsonResult> GetProfilesMessagesCounts()
        {
            try
            {
                var response =
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
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Error getting profiles messages counts.");
            }

            return Json(
                new { Success = false },
                JsonRequestBehavior.AllowGet);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleReceivedMessages", eLeftMenu.ReceivedMessages)]
        public async Task<ActionResult> Inbox(int page = 1)
        {
            SearchMessagesViewModel searchFilter =
                this.GetTempModel<SearchMessagesViewModel>(false)
                    ?? this.InitSearchFilter();

            InboxResponse response =
                await this.messageClient.Value.InboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        TitleQuery = searchFilter.Title,
                        ProfileNameQuery = searchFilter.Subject,
                        FromDate = searchFilter.FromDate?.ToTimestamp(),
                        ToDate = searchFilter.ToDate?.ToTimestamp(),
                        Orn = searchFilter.Orn,
                        ReferencedOrn = searchFilter.ReferencedOrn,
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
                    SearchFilter = searchFilter
                };

            if (model.Messages.Count == 0)
            {
                ViewBag.NoMessages = searchFilter.HasFilter
                    ? ProfilePage.LabelNoMessagesFromSearch
                    : ProfilePage.LabelNoReceivedMessages;
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/_InboxMessageList", model);
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public ActionResult Inbox()
        {
            SearchMessagesViewModel searchFilter = this.InitSearchFilter();

            this.SetTempModel(searchFilter, false);

            return RedirectToAction("Inbox", new { page = 1 });
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleSentMessages", eLeftMenu.SentMessages)]
        public async Task<ActionResult> Outbox(int page = 1)
        {
            SearchMessagesViewModel searchFilter =
                this.GetTempModel<SearchMessagesViewModel>(false)
                    ?? this.InitSearchFilter();

            OutboxResponse response =
                await this.messageClient.Value.OutboxAsync(
                    new BoxRequest
                    {
                        ProfileId = this.UserData.ActiveProfileId,
                        LoginId = this.UserData.LoginId,
                        Offset = (page - 1) * SystemConstants.PageSize,
                        Limit = SystemConstants.PageSize,
                        TitleQuery = searchFilter.Title,
                        ProfileNameQuery = searchFilter.Subject,
                        FromDate = searchFilter.FromDate?.ToTimestamp(),
                        ToDate = searchFilter.ToDate?.ToTimestamp(),
                        Orn = searchFilter.Orn,
                        ReferencedOrn = searchFilter.ReferencedOrn,
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
                    SearchFilter = searchFilter
                };

            if (model.Messages.Count == 0)
            {
                ViewBag.NoMessages = searchFilter.HasFilter
                    ? ProfilePage.LabelNoMessagesFromSearch
                    : ProfilePage.LabelNoSentMessages;
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("Partials/_OutboxMessageList", model);
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.ListProfileMessage)]
        [HttpPost]
        public ActionResult Outbox()
        {
            SearchMessagesViewModel searchFilter = this.InitSearchFilter();

            this.SetTempModel(searchFilter, false);

            return RedirectToAction("Outbox", new { page = 1 });
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
            Policy = Policies.ReadMessageAsRecipient,
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
            await messageClient.Value.OpenAsync(
                new OpenRequest
                {
                    LoginId = UserData.LoginId,
                    MessageId = messageId,
                    ProfileId = UserData.ActiveProfileId
                },
                cancellationToken: Response.ClientDisconnectedToken);

            ReadResponse response =
                await messageClient.Value.ReadAsync(
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
                await messageClient.Value.ViewAsync(
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
            [Bind(Prefix = "id")] int messageId)
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
                    new { id = messageId, templateId = reply.ResponseTemplateId.Value });
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
                    new { id = messageId, templateId = replyTemplateId });
            }

            ChooseReplyViewModel vm = new ChooseReplyViewModel
            {
                MessageId = messageId,
                Templates = templatesVM
            };

            return View(vm);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.WriteMessage,
            TemplateIdRouteOrQueryParam = "templateId")]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessage", eLeftMenu.CreateMessage)]
        public async Task<ActionResult> Reply(
            [Bind(Prefix = "id")] int messageId,
            int templateId)
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
                    ReferencedOrn = reply.Orn,
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
                TemplateId = templateId
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

                    foreach (var error in templateValidationErrors)
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
                        ReferencedOrn = model.ReferencedOrn,
                        AdditionalIdentifier = model.AdditionalIdentifier,
                    };

                _ = await messageClient.Value.SendAsync(
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
                    "Can not send message from login {0} to profile Ids {1}!",
                    User.Identity.Name,
                    model.RecipientIds);

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
                    ForwardTemplateId = SystemForwardTemplateId
                };

            return PartialView("Partials/_Forward", model);
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

                    foreach (var error in templateValidationErrors)
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
                    return PartialView("Partials/_Forward", model);
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
                        ReferencedOrn = model.ReferencedOrn,
                        AdditionalIdentifier = model.AdditionalIdentifier,
                    };

                _ = await messageClient.Value.SendAsync(
                    request,
                    cancellationToken: Response.ClientDisconnectedToken);

                return PartialView("Partials/_ForwardMessageSuccess");
            }
            catch (RpcException er)
            {
                ElmahLogger.Instance.Error(er, er.Message);

                ModelState.AddModelError(
                    "ForwardRecipientProfileId",
                    er.Message);

                return PartialView("Partials/_Forward", model);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Can not forward message from login {0} to profile Id {1}!",
                    User.Identity.Name,
                    model.ForwardRecipientProfileId);

                ModelState.AddModelError(
                    "ForwardRecipientProfileId",
                    ErrorMessages.ErrorCantSendDocument);

                return PartialView("Partials/_Forward", model);
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

        private SearchMessagesViewModel InitSearchFilter()
        {
            SearchMessagesViewModel searchFilter = new SearchMessagesViewModel
            {
                Title = this.Request[nameof(SearchMessagesViewModel.Title)]
                    ?? string.Empty,
                Subject = this.Request[nameof(SearchMessagesViewModel.Subject)]
                    ?? string.Empty,
                Orn = this.Request[nameof(SearchMessagesViewModel.Orn)]
                    ?? string.Empty,
                ReferencedOrn = this.Request[nameof(SearchMessagesViewModel.ReferencedOrn)]
                    ?? string.Empty,
                FromDateAsString = this.Request[nameof(SearchMessagesViewModel.FromDateAsString)]
                    ?? string.Empty,
                ToDateAsString = this.Request[nameof(SearchMessagesViewModel.ToDateAsString)]
                    ?? string.Empty
            };

            bool parseFromDate = DateTime.TryParseExact(
                searchFilter.FromDateAsString,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime fromDate);

            bool parseToDate = DateTime.TryParseExact(
                searchFilter.ToDateAsString,
                SystemConstants.DatePickerDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime toDate);

            if (parseFromDate)
            {
                searchFilter.FromDate = fromDate;
            }

            if (parseToDate)
            {
                searchFilter.ToDate = toDate.AddDays(1);
            }

            return searchFilter;
        }

        private Dictionary<int, int> GetSEOSNewMessagesCount()
        {
            if (this.UserData.Profiles.Any(e => e.HasSEOS ?? false))
            {
                try
                {
                    using (var seosClient = new SEOSPostServiceClient())
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
