using System;
using System.Globalization;
using System.Threading;
using System.Web;

using EDelivery.WebPortal.Enums;

namespace EDelivery.WebPortal.Utils
{
    public static class CultureHelper
    {
        private static readonly CultureInfo bgCulture = (CultureInfo)CultureInfo.GetCultureInfo("bg-BG").Clone();
        private static readonly CultureInfo enCulture = (CultureInfo)CultureInfo.GetCultureInfo("en-US").Clone();
        private static readonly string cookieName = "culture-edev.egov.bg";

        public static eSiteCulture ToSiteCulture(string culture)
        {
            switch (culture)
            {
                case "en-US":
                    return eSiteCulture.EN;
                case "bg-BG":
                    return eSiteCulture.BG;
                default:
                    return eSiteCulture.Invariant;
            }
        }

        internal static void ApplyCulture(HttpContext httpContext)
        {
            HttpCookie cookie = httpContext.Request.Cookies[cookieName];
            if (cookie == null)
            {
                return;
            }

            string value = cookie.Value;

            if (Enum.TryParse(value, out eSiteCulture result)
                && ToSiteCulture(Thread.CurrentThread.CurrentUICulture.Name) != result)
            {
                switch (result)
                {
                    case eSiteCulture.EN:
                        enCulture.DateTimeFormat.ShortDatePattern =
                            SystemConstants.DatePickerDateFormat;

                        Thread.CurrentThread.CurrentCulture =
                            Thread.CurrentThread.CurrentUICulture = enCulture;

                        break;
                    case eSiteCulture.BG:
                    case eSiteCulture.Invariant:
                        bgCulture.DateTimeFormat.ShortDatePattern =
                            SystemConstants.DatePickerDateFormat;

                        Thread.CurrentThread.CurrentCulture =
                            Thread.CurrentThread.CurrentUICulture = bgCulture;

                        break;
                }
            }
        }

        internal static void ChangeCulture(HttpContext httpContext, eSiteCulture culture)
        {
            if (ToSiteCulture(Thread.CurrentThread.CurrentUICulture.Name) != culture)
            {
                HttpCookie cookie = httpContext.Request.Cookies[cookieName];
                if (cookie == null)
                {
                    cookie = new HttpCookie(cookieName);
                }

                cookie.Value = culture.ToString();
                cookie.HttpOnly = true;
                cookie.Secure = true;
                cookie.Expires = DateTime.Now.AddMonths(1);

                httpContext.Response.Cookies.Add(cookie);
            }
        }
    }
}
