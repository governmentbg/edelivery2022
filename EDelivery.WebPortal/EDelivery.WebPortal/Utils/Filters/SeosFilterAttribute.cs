using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Filters
{
    public class SeosFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            CachedUserData userData = filterContext
                .RequestContext
                .HttpContext
                .GetCachedUserData();

            if (userData.ActiveProfile.TargetGroupId != (int)Enums.TargetGroupId.PublicAdministration
                || !userData.ActiveProfile.HasSEOS.HasValue
                || !userData.ActiveProfile.HasSEOS.Value)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new
                        {
                            controller = "Profile",
                            action = "Index"
                        }));
            }
        }
    }
}