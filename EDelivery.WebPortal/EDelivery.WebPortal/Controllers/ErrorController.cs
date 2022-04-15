using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EDelivery.WebPortal.Controllers
{
    [AllowAnonymous]
    public class ErrorController : BaseController
    {
        [Route("Error/{id?}")]
        public ActionResult Index(string id)
        {
            Models.HandleErrorInfoWithMessage model;
            switch(id)
            {
                case "403":
                    model = new Models.HandleErrorInfoWithMessage(EDeliveryResources.ErrorMessages.ErrorPageForbidden);
                    break;
                case "404":
                    model= new Models.HandleErrorInfoWithMessage( EDeliveryResources.ErrorMessages.ErrorPageNotFound);
                    break;
                default:
                    model = new Models.HandleErrorInfoWithMessage( EDeliveryResources.ErrorMessages.ErrorSystemGeneral);
                    break;
            }
            return View("Error", model);
        }
    }
}