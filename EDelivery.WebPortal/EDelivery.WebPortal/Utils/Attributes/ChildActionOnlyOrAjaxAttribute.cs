using System;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Utils.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ChildActionOnlyOrAjaxAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (!filterContext.IsChildAction
                && !filterContext.RequestContext.HttpContext.Request.IsAjaxRequest())
            {
                throw new InvalidOperationException(
                    $"ChildActionOnlyOrAjax {filterContext.ActionDescriptor.ActionName}");
            }
        }
    }
}