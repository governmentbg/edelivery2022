using System.Web.Mvc;

namespace EDelivery.WebPortal.Areas.MetaData
{
    public class MetaDataAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MetaData";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "MetaData_default",
                "MetaData/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}