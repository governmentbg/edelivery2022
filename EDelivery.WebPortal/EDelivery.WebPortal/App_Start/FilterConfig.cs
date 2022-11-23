using System.Web.Mvc;

namespace EDelivery.WebPortal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequireHttpsAttribute());
        }
    }
}
