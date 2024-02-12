using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace EDelivery.WebPortal.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HandleHttpAntiForgeryExceptionAttribute : FilterAttribute, IExceptionFilter
    {
        public virtual void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            Exception exception = filterContext.Exception;

            if (!filterContext.HttpContext.User.Identity.IsAuthenticated
                && exception is HttpAntiForgeryException)
            {
                filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "action", "Index" },
                    { "controller", "Home" }
                });

                filterContext.ExceptionHandled = true;
            }
        }
    }
}
