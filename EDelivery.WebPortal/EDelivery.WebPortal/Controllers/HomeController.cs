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
using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Controllers
{
    [AllowAnonymous]
    public class HomeController : BaseController
    {
        private readonly Lazy<Profile.ProfileClient> profileClient;

        private readonly Lazy<CachedProfileStatisticsData> profileStatisticsData;

        public HomeController()
        {
            this.profileClient = new Lazy<Profile.ProfileClient>(
                () => Grpc.GrpcClientFactory.CreateProfileClient(), isThreadSafe: false);

            this.profileStatisticsData = new Lazy<CachedProfileStatisticsData>(
                () => this.HttpContext.GetCachedProfileStatistics(), isThreadSafe: false);
        }

        [HttpGet]
        public ActionResult Index(string returnUrl)
        {
            return View(new IndexViewModel { ReturnUrl = returnUrl });
        }

        [HttpGet]
        public ActionResult New()
        {
            return View();
        }

        [ChildActionOnlyOrAjax]
        [HttpPost]
        public ActionResult GetStatistics()
        {
            StatisticsViewModel vm = null;

            try
            {
                vm = new StatisticsViewModel
                {
                    LegalEntitiesCount = this.profileStatisticsData.Value.LegalEntitiesCount,
                    PublicAdministrationsCount = this.profileStatisticsData.Value.PublicAdministrationsCount,
                    SocialOrganizationsCount = this.profileStatisticsData.Value.SocialOrganizationsCount,
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
        [HttpGet]
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
        [ChildActionOnlyOrAjax]
        [HttpPost]
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
        [HttpGet]
        public ActionResult RobotsText()
        {
            // https://weblog.west-wind.com/posts/2015/nov/13/serving-urls-with-file-extensions-in-an-aspnet-mvc-application
            return Utils.Utils.IsProductionEnvironment
                ? File(Server.MapPath("~/App_Data/RobotsText/robots.ProdV2.txt"), "text/plain")
                : File(Server.MapPath("~/App_Data/RobotsText/robots.txt"), "text/plain");
        }
    }
}
