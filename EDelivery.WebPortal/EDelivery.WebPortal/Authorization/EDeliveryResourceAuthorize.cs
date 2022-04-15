using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using EDelivery.WebPortal.Utils;

using Microsoft.Owin;
using Microsoft.Owin.Security.Authorization.Mvc;

namespace EDelivery.WebPortal.Authorization
{
    public class EDeliveryResourceAuthorize : ResourceAuthorizeAttribute
    {
        public string MessageIdRouteOrQueryParam { get; set; }

        public string MessageIdFormValueParam { get; set; }

        public string TemplateIdRouteOrQueryParam { get; set; }

        public string RecipientGroupIdRouteOrQueryParam { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // do not authorize child actions, the parent action should do that
            if (filterContext.IsChildAction)
            {
                return;
            }

            RequirementContext requirementContext =
                AuthorizationHelper.GetRequirementContext(filterContext);

            IOwinContext owinContext = filterContext.HttpContext.GetOwinContext();

            requirementContext.Set(
                RequirementContextItems.LoginId,
                // delay the cached data loginId retrieval as
                // we only want to access it when the user is authenticated
                () => owinContext.GetCachedUserData()?.LoginId);

            requirementContext.Set(
                RequirementContextItems.ProfileId,
                // delay the cached data profileId retrieval as
                // we only want to access it when the user is authenticated
                () => owinContext.GetCachedUserData()?.ActiveProfileId);

            if (!string.IsNullOrEmpty(this.MessageIdRouteOrQueryParam))
            {
                requirementContext.Set(
                    RequirementContextItems.MessageId,
                    filterContext.GetInt32RouteOrQueryParam(this.MessageIdRouteOrQueryParam));
            }

            if (!string.IsNullOrEmpty(this.MessageIdFormValueParam))
            {
                requirementContext.Set(
                    RequirementContextItems.MessageId,
                    filterContext.GetInt32FormValue(this.MessageIdFormValueParam));
            }

            if (!string.IsNullOrEmpty(this.TemplateIdRouteOrQueryParam))
            {
                requirementContext.Set(
                    RequirementContextItems.TemplateId,
                    filterContext.GetInt32RouteOrQueryParam(this.TemplateIdRouteOrQueryParam));
            }

            if (!string.IsNullOrEmpty(this.RecipientGroupIdRouteOrQueryParam))
            {
                requirementContext.Set(
                    RequirementContextItems.RecipientGroupId,
                    filterContext.GetInt32RouteOrQueryParam(this.RecipientGroupIdRouteOrQueryParam));
            }

            base.OnAuthorization(filterContext);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(403);

                    return;
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(
                            new
                            {
                                controller = "Error",
                                action = "Index",
                                id = "403"
                            }));

                    return;
                }
            }
            else
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new HttpStatusCodeResult(401);

                    return;
                }
            }

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
