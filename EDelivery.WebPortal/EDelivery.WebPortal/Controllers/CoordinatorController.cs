using System;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;

using EDelivery.WebPortal.Authorization;

namespace EDelivery.WebPortal.Controllers
{
    [EDeliveryResourceAuthorize]
    public class CoordinatorController : BaseController
    {
        [HttpGet]
        public ActionResult Index(string t)
        {
            try
            {
                if (string.IsNullOrEmpty(t))
                {
                    return RedirectToAction("Index", "Home");
                }

                string tokenSecret =
                    WebConfigurationManager.AppSettings["CoordinatorTokenSecret"];

                string aesSecret =
                    WebConfigurationManager.AppSettings["CoordinatorAesSecret"];

                CoordinatorPayload tokenObject =
                    JwtService.ParseCoordinatorJwt(t, tokenSecret, aesSecret);

                if (tokenObject == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                int activeProfileId = this.UserData.ActiveProfileId;
                int switchProfileId = this.UserData.Profiles
                    .Where(x => x.Identifier == tokenObject.ProfileId)
                    .Select(y => y.ProfileId)
                    .FirstOrDefault();

                if (switchProfileId != 0 || switchProfileId != activeProfileId)
                {
                    this.UserData.ActiveProfileId = switchProfileId;
                    this.UserData.BreadCrumb.ProfileName =
                        this.UserData.ActiveProfile.ProfileName;
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }

                return Redirect(tokenObject.RedirectUrl);
            }
            catch (Exception ex)
            {
                ElmahLogger.Instance.Error(ex, "Error redirecting to web portal!");

                return RedirectToAction("Index", "Home");
            }
        }
    }
}
