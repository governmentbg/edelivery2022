using System;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace EDelivery.WebPortal.Extensions
{
    public static class AjaxHtml
    {
        public static MvcHtmlString RawActionLink(
            this AjaxHelper ajaxHelper,
            string linkText,
            string actionName,
            string controllerName,
            object routeValues,
            AjaxOptions ajaxOptions,
            object htmlAttributes)
        {
            string repID = Guid.NewGuid().ToString();

            MvcHtmlString lnk = ajaxHelper.ActionLink(
                repID,
                actionName,
                controllerName,
                routeValues,
                ajaxOptions,
                htmlAttributes);

            return MvcHtmlString.Create(lnk.ToString().Replace(repID, linkText));
        }
    }
}