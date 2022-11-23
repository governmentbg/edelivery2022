using System;
using System.Web.Mvc;
using EDelivery.WebPortal.Utils;

using log4net;

namespace EDelivery.WebPortal.Controllers
{
    public class BaseController : Controller
    {
        protected static ILog logger = LogManager.GetLogger("Web.Controller");

        public CachedUserData UserData
        {
            get
            {
                return this.HttpContext.GetCachedUserData();
            }
        }
    }
}
