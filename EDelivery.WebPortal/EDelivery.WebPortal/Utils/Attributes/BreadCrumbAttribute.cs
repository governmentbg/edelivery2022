using System;
using System.Web.Mvc;

using EDelivery.WebPortal.Enums;
using EDelivery.WebPortal.Models;

namespace EDelivery.WebPortal.Utils.Attributes
{
    public class BreadCrumbAttribute : ActionFilterAttribute
    {
        public int MaxLinkLevel { get; set; }

        public string LinkText { get; set; }

        public eLeftMenu ELeftMenu { get; set; } = eLeftMenu.None;

        public bool ResetState { get; set; }

        public BreadCrumbAttribute(
            int maxLinkLevel,
            Type resourceType,
            string resourceName,
            eLeftMenu eLeftMenu,
            bool reset = false)
        {
            this.MaxLinkLevel = maxLinkLevel;
            this.LinkText = new System.Resources.ResourceManager(resourceType)
                .GetString(resourceName);
            this.ELeftMenu = eLeftMenu;
            this.ResetState = reset;
        }

        public BreadCrumbAttribute(
            int maxLinkLevel,
            string title,
            eLeftMenu eLeftMenu)
        {
            this.MaxLinkLevel = maxLinkLevel;
            this.LinkText = title;
            this.ELeftMenu = eLeftMenu;
            this.ResetState = false;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.RequestContext.HttpContext.Request.IsAuthenticated)
            {
                return;
            }

            CachedUserData userData = filterContext.RequestContext.HttpContext.GetCachedUserData();

            BreadCrumb userBreadCrumb = userData.BreadCrumb;
            userBreadCrumb.ELeftMenu = this.ELeftMenu;

            while (userBreadCrumb.Links.Count >= this.MaxLinkLevel)
            {
                userBreadCrumb.Links.RemoveAt(userBreadCrumb.Links.Count - 1);
            }

            if (!ResetState)
            {
                BreadCrumbLink newLink = new BreadCrumbLink()
                {
                    LinkName = this.LinkText,
                    LinkUrl = Utils.GetActionUrl(
                        filterContext.RouteData.Values["action"].ToString(),
                        filterContext.RouteData.Values["controller"].ToString(),
                        null)
                };

                userBreadCrumb.Links.Add(newLink);
            }
        }
    }
}
