using System.Web.Mvc;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Utils.Attributes;

namespace EDelivery.WebPortal.Controllers
{
    [AllowAnonymous]
    public class HelpController : BaseController
    {
        [BreadCrumb(2, typeof(EDeliveryResources.Common), "TitleHelp", eLeftMenu.None)]
        public ActionResult Index()
        {
            var executingAssembly = this.GetType().Assembly;
            ViewBag.SiteVersion = string.Format(
                "{0} - {1:dd MMM yyyy HH:mm:ss}",
                executingAssembly.GetName().Version,
                System.IO.File.GetLastWriteTime(executingAssembly.Location));

            return View();
        }
    }
}