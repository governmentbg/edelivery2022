using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Filters
{
    public class StripAuthCookieAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.GetOwinContext()
                .Environment
                .Add(AuthConfig.StripEDeliveryIdentityCookieName, true);

            base.OnActionExecuting(filterContext);
        }
    }
}
