using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

using ED.DomainServices.CodeMessages;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Extensions;
using EDelivery.WebPortal.Grpc;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.Templates;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Attributes;
using EDelivery.WebPortal.Utils.Exceptions;

using EDeliveryResources;

using Grpc.Core;

using Newtonsoft.Json;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class CodeMessagesController : BaseController
    {
        private const string MessageCodeSessionKey = "MessageCodeSessionKey";
        private const string MessageCodeTempDataKey = "MessageCodeTempDataKey";
        private const int SystemTemplateId = 1;
        private const int SystemLoginId = 1;

        private readonly Lazy<ED.DomainServices.Messages.Message.MessageClient> messageClient;
        private readonly Lazy<CodeMessage.CodeMessageClient> codeMessageClient;
        private readonly Lazy<CachedUserData> userData;

        public CodeMessagesController()
        {
            this.messageClient =
                new Lazy<ED.DomainServices.Messages.Message.MessageClient>(
                    () => GrpcClientFactory.CreateMessageClient(), isThreadSafe: false);

            this.codeMessageClient =
                new Lazy<CodeMessage.CodeMessageClient>(
                    () => GrpcClientFactory.CreateCodeMessageClient(), isThreadSafe: false);

            this.userData =
                new Lazy<CachedUserData>(
                    () => this.HttpContext.GetCachedUserData(), isThreadSafe: false);
        }

        [HttpGet]
        [AllowAnonymous]
        [BreadCrumb(3, typeof(EDeliveryResources.Common), "TitleOpenMessageWithAccessCode", eLeftMenu.ReceivedMessages)]
        public ActionResult Index()
        {
            OpenCodeMessageViewModel model = new OpenCodeMessageViewModel();

            OpenCodeMessageViewModel postModel =
                this.GetTempModel<OpenCodeMessageViewModel>(true);

            if (postModel != null)
            {
                model = postModel;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [BreadCrumb(3, typeof(EDeliveryResources.Common), "TitleOpenMessageWithAccessCode", eLeftMenu.ReceivedMessages)]
        public ActionResult Index(OpenCodeMessageViewModel model)
        {
            if (!Guid.TryParse(Regex.Replace(model.AccessCode, @"[\-\s]+", ""), out Guid accessCode))
            {
                ModelState.AddModelError(
                    nameof(model.AccessCode),
                    ErrorMessages.ErrorInvalidAccessCode);
            }

            if (!ModelState.IsValid)
            {
                this.SetTempModel(model, true);

                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            this.TempData[MessageCodeTempDataKey] = accessCode;

            return RedirectToAction(nameof(CodeMessagesController.Open));
        }

        [HttpGet]
        [AllowAnonymous]
        [BreadCrumb(3, typeof(EDeliveryResources.Common), "TitleOpenMessageWithAccessCode", eLeftMenu.ReceivedMessages)]
        public async Task<ActionResult> Open()
        {
            Guid accessCode;
            if (this.TempData[MessageCodeTempDataKey] is Guid tempDataAccessCode)
            {
                accessCode = tempDataAccessCode;
            }
            else if (this.Session[MessageCodeSessionKey] is Guid sessionAccessCode)
            {
                accessCode = sessionAccessCode;
            }
            else
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            try
            {
                OpenResponse resp =
                    await codeMessageClient.Value.OpenAsync(
                        new OpenRequest
                        {
                            AccessCode = accessCode.ToString()
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                if (!resp.IsSuccessful)
                {
                    throw new ModelStateException(
                        nameof(OpenCodeMessageViewModel.AccessCode),
                        ErrorMessages.ErrorInvalidAccessCode);
                }

                ReadResponse readMessage =
                    await codeMessageClient.Value.ReadAsync(
                        new ReadRequest
                        {
                            MessageId = resp.MessageId.Value,
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                ReadCodeMessageViewModel vm =
                    new ReadCodeMessageViewModel(readMessage.Message);

                ED.DomainServices.Messages.GetTemplateContentResponse template =
                    await this.messageClient.Value.GetTemplateContentAsync(
                        new ED.DomainServices.Messages.GetTemplateContentRequest
                        {
                            TemplateId = vm.TemplateId
                        },
                        cancellationToken: Response.ClientDisconnectedToken);

                Dictionary<Guid, FieldObject> valueDict =
                    TemplatesService.GetFields(
                        vm.Body,
                        template.Content);

                vm.SetFields(valueDict);

                // store the accessCode in the session for successful opens only
                this.Session[MessageCodeSessionKey] = accessCode;

                return View(vm);
            }
            catch (ModelStateException mse)
            {
                ElmahLogger.Instance.Error(
                    mse,
                    $"Message with access code {accessCode} does not exists");

                ModelState.AddModelError(mse.Key, mse.Message);

                OpenCodeMessageViewModel model = new OpenCodeMessageViewModel
                {
                    AccessCode = accessCode.ToString()
                };

                this.SetTempModel(model, true);

                return RedirectToAction(nameof(CodeMessagesController.Index));
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Exception in open message with code");

                ModelState.AddModelError(
                    nameof(OpenCodeMessageViewModel.AccessCode),
                    ErrorMessages.ErrorSystemGeneral);

                OpenCodeMessageViewModel model = new OpenCodeMessageViewModel
                {
                    AccessCode = accessCode.ToString()
                };

                this.SetTempModel(model, true);

                return RedirectToAction(nameof(CodeMessagesController.Index));
            }
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.WriteCodeMessage)]
        [HttpGet]
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleCreateNewMessageWithCode", eLeftMenu.CreateMessageWithCode)]
        public ActionResult New()
        {
            NewCodeMessageViewModel model = new NewCodeMessageViewModel
            {
                CurrentProfileId = UserData.ActiveProfileId,
                TemplateId = SystemTemplateId
            };

            NewCodeMessageViewModel postModel =
                this.GetTempModel<NewCodeMessageViewModel>(true);

            if (postModel != null)
            {
                model = postModel;
            }

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(Policy = Policies.WriteCodeMessage)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> New(
            NewCodeMessageViewModel model,
            FormCollection form)
        {
            try
            {
                ED.DomainServices.Messages.GetTemplateContentResponse templateContent =
                    await this.messageClient.Value.GetTemplateContentAsync(
                        new ED.DomainServices.Messages.GetTemplateContentRequest
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

                    return RedirectToAction("New");
                }

                (string jsonMetaFields, string jsonPayload, int[] blobIds) =
                    TemplatesService.ExtractPayloads(templateContent.Content, form);

                SendRequest request = new SendRequest
                {
                    Identifier = model.Identifier,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Email = model.Email,
                    SenderLoginId = this.UserData.LoginId,
                    SenderProfileId = this.UserData.ActiveProfileId,
                    TemplateId = model.TemplateId,
                    Subject = model.Subject,
                    Body = jsonPayload,
                    MetaFields = jsonMetaFields,
                    BlobIds = { blobIds },
                };

                _ = await codeMessageClient.Value.SendAsync(
                    request,
                    cancellationToken: Response.ClientDisconnectedToken);

                return RedirectToAction("Outbox", "Messages");
            }
            catch (RpcException er)
            {
                ElmahLogger.Instance.Error(er, er.Message);

                ModelState.AddModelError(string.Empty, ErrorMessages.ErrorCantSendDocument);

                this.SetTempModel(model, true);

                return RedirectToAction("New");
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Can not send message from login {User.Identity.Name} to profile with identifier {model.Identifier}!");

                ModelState.AddModelError(string.Empty, ErrorMessages.ErrorCantSendDocument);

                this.SetTempModel(model, true);

                return RedirectToAction("New");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> MessageProfileInfo()
        {
            if (!(this.Session[MessageCodeSessionKey] is Guid))
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            Guid accessCode = (Guid)this.Session[MessageCodeSessionKey];

            GetSenderProfileResponse profile =
                await this.codeMessageClient.Value.GetSenderProfileAsync(
                    new GetSenderProfileRequest
                    {
                        AccessCode = accessCode.ToString()
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            CodeMessageProfileInfoViewModel vm =
                new CodeMessageProfileInfoViewModel(profile);

            return PartialView("Partials/_MessageProfileInfo", vm);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetMessageTimestamp(eTimeStampType timeStampType)
        {
            if (!(this.Session[MessageCodeSessionKey] is Guid))
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            Guid accessCode = (Guid)this.Session[MessageCodeSessionKey];

            GetTimestampResponse response;

            switch (timeStampType)
            {
                case eTimeStampType.NRO:
                    response = await this.codeMessageClient.Value.GetTimestampNROAsync(
                        new GetTimestampRequest
                        {
                            AccessCode = accessCode.ToString(),
                        },
                        cancellationToken: Response.ClientDisconnectedToken);
                    break;
                case eTimeStampType.NRD:
                    response = await this.codeMessageClient.Value.GetTimestampNRDAsync(
                        new GetTimestampRequest
                        {
                            AccessCode = accessCode.ToString(),
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

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetDocumentTimestamp(int documentId)
        {
            if (!(this.Session[MessageCodeSessionKey] is Guid))
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            Guid accessCode = (Guid)this.Session[MessageCodeSessionKey];

            GetBlobTimestampResponse response =
                await this.codeMessageClient.Value.GetBlobTimestampAsync(
                    new GetBlobTimestampRequest
                    {
                        AccessCode = accessCode.ToString(),
                        BlobId = documentId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Timestamp.ToArray(),
                "application/octet-stream",
                response.FileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetPdfAsRecipient()
        {
            if (!(this.Session[MessageCodeSessionKey] is Guid))
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            Guid accessCode = (Guid)this.Session[MessageCodeSessionKey];

            GetPdfAsRecipientResponse response =
               await this.codeMessageClient.Value.GetPdfAsRecipientAsync(
                   new GetPdfAsRecipientRequest
                   {
                       AccessCode = accessCode.ToString(),
                   },
                   cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Content.ToArray(),
                response.ContentType,
                response.FileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetSummary()
        {
            if (!(this.Session[MessageCodeSessionKey] is Guid))
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            Guid accessCode = (Guid)this.Session[MessageCodeSessionKey];

            GetSummaryResponse response =
                await this.codeMessageClient.Value.GetSummaryAsync(
                    new GetSummaryRequest
                    {
                        AccessCode = accessCode.ToString(),
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            return File(
                response.Summary.ToArray(),
                response.ContentType,
                response.FileName);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Download([Bind(Prefix = "t")] string token)
        {
            if (!(this.Session[MessageCodeSessionKey] is Guid))
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            Guid accessCode = (Guid)this.Session[MessageCodeSessionKey];

            Guid tokenAccessCode;
            int profileId;
            int messageId;
            int blobId;
            try
            {
                (tokenAccessCode, profileId, messageId, blobId) =
                    BlobUrlCreator.ParseMessageBlobAccessCodeToken(token);
            }
            catch (CryptographicException)
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            if (accessCode != tokenAccessCode)
            {
                return RedirectToAction(nameof(CodeMessagesController.Index));
            }

            return new RedirectResult(
                BlobUrlCreator.CreateMessageBlobUrl(
                    profileId,
                    messageId,
                    blobId));
        }
    }
}
