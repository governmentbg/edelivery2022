using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

using ED.DomainServices.Translations;

using EDelivery.WebPortal.Authorization;
using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Grpc;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Utils;
using EDelivery.WebPortal.Utils.Attributes;

using EDeliveryResources;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class TranslationsController : Controller
    {
        private readonly Lazy<Translation.TranslationClient> translationClient;
        private readonly Lazy<CachedUserData> userData;

        private const int MaxArchivedTranslationsPerMessage = 10;

        public TranslationsController()
        {
            this.translationClient =
                new Lazy<Translation.TranslationClient>(
                    () => GrpcClientFactory.CreateTranslationClient(), isThreadSafe: false);

            this.userData =
                new Lazy<CachedUserData>(
                    () => this.HttpContext.GetCachedUserData(), isThreadSafe: false);
        }

        private CachedUserData UserData => this.userData.Value;

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.MessageAccess,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        [BreadCrumb(4, typeof(EDeliveryResources.Common), "TitleTranslations", eLeftMenu.ReceivedMessages)]
        public ActionResult List(
            [Bind(Prefix = "id")] int messageId)
        {
            TranslationsListViewModel model =
                new TranslationsListViewModel(messageId);

            return View(model);
        }

        [OverrideAuthorization]
        [EDeliveryResourceAuthorize(
            Policy = Policies.MessageAccess,
            MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        [ChildActionOnlyOrAjax]
        public ActionResult ListTranslations(
            [Bind(Prefix = "id")] int messageId)
        {
            GetTranslationsResponse response =
                this.translationClient.Value.GetTranslations(
                    new GetTranslationsRequest
                    {
                        MessageId = messageId,
                        ProfileId = this.UserData.ActiveProfileId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            TranslationsListViewModel model =
                new TranslationsListViewModel(messageId, response);

            if (model.Items.Count == 0)
            {
                ViewBag.NoTranslations = ProfilePage.LabelNoMessageTranslations;
            }

            return PartialView("Partials/ListTranslations", model);
        }

        // TODO authorization
        [AllowAnonymous]
        //[OverrideAuthorization]
        //[EDeliveryResourceAuthorize(
        //    Policy = Policies.MessageAccess,
        //    MessageIdRouteOrQueryParam = "id")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(
            int messageTranslationId,
            int messageId)
        {
            int profileId = this.UserData.ActiveProfileId;

            try
            {
                GetArchivedMessageTranslationsCountResponse resp =
                    await this.translationClient.Value.GetArchivedMessageTranslationsCountAsync(
                        new GetArchivedMessageTranslationsCountRequest
                        {
                            MessageId = messageTranslationId,
                            ProfileId = this.UserData.ActiveProfileId,
                        });

                if (resp.Count > MaxArchivedTranslationsPerMessage)
                {
                    this.Response.StatusCode = 400;
                    return Json(new
                    {
                        error = ErrorMessages.ErrorMaxArchivedMessageTranslations
                    });
                }

                _ = await this.translationClient.Value.ArchiveMessageTranslationAsync(
                    new ArchiveMessageTranslationRequest
                    {
                        MessageTranslationId = messageTranslationId,
                        LoginId = this.UserData.LoginId,
                    });

                return RedirectToAction(
                    nameof(TranslationsController.ListTranslations),
                    new { id = messageId });
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    $"Can not delete translation {messageTranslationId}");

                this.Response.StatusCode = 400;
                return Json(new
                {
                    error = ErrorMessages.ErrorSystemGeneral
                });
            }
        }

        // TODO authorization
        [AllowAnonymous]
        //[OverrideAuthorization]
        //[EDeliveryResourceAuthorize(
        //    Policy = Policies.MessageAccess,
        //    MessageIdRouteOrQueryParam = "id")]
        [HttpGet]
        [BreadCrumb(5, typeof(EDeliveryResources.Common), "TitleTranslationDetails", eLeftMenu.ReceivedMessages)]
        public async Task<ActionResult> Details(
            [Bind(Prefix = "id")] int messageTranslationId)
        {
            GetTranslationResponse response =
                await this.translationClient.Value.GetTranslationAsync(
                    new GetTranslationRequest
                    {
                        MessageTranslationId = messageTranslationId,
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            TranslationViewModel model =
                new TranslationViewModel(response.Translation);

            return View(model);
        }
    }
}
