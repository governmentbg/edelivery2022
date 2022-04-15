using System.Web.Mvc;

using EDelivery.WebPortal.Utils;

using log4net;

namespace EDelivery.WebPortal.Areas.MetaData.Controllers
{
    [AllowAnonymous]
    public class InfoController : Controller
    {
        private static ILog logger = LogManager.GetLogger("Metadata");

        public ActionResult Saml()
        {
            logger.Info("Request to SAML metadata");

            string saml = SamlHelper.GetSPMetdata();

            return Content(saml, "application/xml", System.Text.Encoding.UTF8);
        }
    }
}