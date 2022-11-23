using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using ED.DomainServices.Profiles;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Models;
using EDelivery.WebPortal.Models.Home;
using EDelivery.WebPortal.Utils;

namespace EDelivery.WebPortal.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly Lazy<Profile.ProfileClient> profileClient;

        public HomeController()
        {
            this.profileClient = new Lazy<Profile.ProfileClient>(
                () => Grpc.GrpcClientFactory.CreateProfileClient(), isThreadSafe: false);
        }

        public ActionResult Index(string returnUrl)
        {
            return View(new IndexViewModel { ReturnUrl = returnUrl });
        }

        public ActionResult New(string returnUrl)
        {
            return View();
        }

        [OutputCache(Duration = 120)]
        [ChildActionOnly]
        public ActionResult GetStatistics()
        {
            StatisticsViewModel vm = null;

            try
            {
                GetStatisticsResponse response =
                    this.profileClient.Value.GetStatistics(
                        new Google.Protobuf.WellKnownTypes.Empty(),
                        cancellationToken: Response.ClientDisconnectedToken);

                vm = new StatisticsViewModel
                {
                    PublicAdministrationsCount = response.PublicAdministrationsCount,
                    SocialOrganizationsCount = response.SocialOrganizationsCount,
                    LegalEntitiesCount = response.LegalEntitiesCount
                };
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(
                    ex,
                    "Can not get statistics!");
            }

            return PartialView("Partials/_HomeStatistics", vm);
        }

        /// <summary>
        /// Gets a list of registered members of a type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Registered(eRegisteredSubjectsType? id)
        {
            if (id == null || id == eRegisteredSubjectsType.Person)
            {
                id = eRegisteredSubjectsType.Administration;
            };

            RegisteredSubjectsModel model = new RegisteredSubjectsModel()
            {
                Type = id
            };

            return View(model);
        }

        [OutputCache(NoStore = true, Duration = 0, Location = System.Web.UI.OutputCacheLocation.Any)]
        public async Task<ActionResult> GetRegisteredOfType(
            eRegisteredSubjectsType type,
            string search,
            int page = 1,
            int pageSize = SystemConstants.LargePageSize)
        {
            if (!Request.IsAjaxRequest() && !this.ControllerContext.IsChildAction)
            {
                return RedirectToAction("Registered", new { id = type, search });
            }

            int targetGroupId = (int)TargetGroupId.PublicAdministration;

            switch (type)
            {
                case eRegisteredSubjectsType.Administration:
                    targetGroupId = (int)TargetGroupId.PublicAdministration;
                    break;
                case eRegisteredSubjectsType.SocialOrganisation:
                    targetGroupId = (int)TargetGroupId.SocialOrganization;
                    break;
                case eRegisteredSubjectsType.LegalPerson:
                    targetGroupId = (int)TargetGroupId.LegalEntity;
                    break;
                default:
                    throw new Exception("Unsupported target group");
            }

            GetTargetGroupProfilesResponse response =
                await this.profileClient.Value.GetTargetGroupProfilesAsync(
                    new GetTargetGroupProfilesRequest
                    {
                        TargetGroupId = targetGroupId,
                        Term = search?.ToLowerInvariant() ?? string.Empty,
                        Offset = (page - 1) * pageSize,
                        Limit = pageSize
                    },
                    cancellationToken: Response.ClientDisconnectedToken);

            List<RegisteredSubjectsItemModel> items = response
                    .Result
                    .Select(e => new RegisteredSubjectsItemModel(e))
                    .ToList();

            RegisteredSubjectsListModel model =
                new RegisteredSubjectsListModel(type)
                {
                    Search = search,
                    Items = new PagedList.PagedListLight<RegisteredSubjectsItemModel>(
                        items,
                        pageSize,
                        page,
                        response.Length)
                };

            return PartialView("Partials/_RegisteredSubjectsList", model);
        }

        // TODO: remove?
        [Route("~/Validation/EIDResult")]
        public ActionResult RedirectToEValidation(
            string Target,
            string URL,
            string SAMLArtifact)
        {
            ElmahLogger.Instance.Error("new request for EValidationAuth - artifact is" + SAMLArtifact);

            string eValidationUrl =
                ConfigurationManager.AppSettings["EValidationURL"];
            if (!string.IsNullOrEmpty(eValidationUrl))
            {
                string url = string.Format(
                    "{0}?Target={1}&URL={2}&SAMLArtifact={3}",
                    eValidationUrl,
                    Url.Encode(Target),
                    Url.Encode(URL),
                    Url.Encode(SAMLArtifact));

                return RedirectPermanent(url);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ChangeCulture(eSiteCulture culture)
        {
            CultureHelper.ChangeCulture(this.HttpContext.ApplicationInstance.Context, culture);

            if (this.Request.UrlReferrer != null)
            {
                return Redirect(this.Request.UrlReferrer.AbsoluteUri);
            }

            return RedirectToAction("Index");
        }

        [Route("robots.txt", Name = "GetRobotsText"), OutputCache(Duration = 86400)]
        public ActionResult RobotsText()
        {
            // https://weblog.west-wind.com/posts/2015/nov/13/serving-urls-with-file-extensions-in-an-aspnet-mvc-application
            return Utils.Utils.IsProductionEnvironment
                ? File(Server.MapPath("~/App_Data/RobotsText/robots.ProdV2.txt"), "text/plain")
                : File(Server.MapPath("~/App_Data/RobotsText/robots.txt"), "text/plain");
        }
    }
}
